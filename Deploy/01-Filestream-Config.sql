-- ============================================================================
-- TurismoEstancia — Configuração do FILESTREAM (executar no servidor de produção)
-- ----------------------------------------------------------------------------
-- A tabela [Arquivos] já nasce preparada para FILESTREAM pelo padrão adotado:
--   • ArquId          bigint identity (PK)
--   • ArquUID         uniqueidentifier NOT NULL DEFAULT NEWID()  → ROWGUIDCOL
--   • ArquFileName    nvarchar(255)
--   • ArquContentType nvarchar(100)
--   • ArquSize        bigint  (DATALENGTH do binário)
--   • ArquBytes       varbinary(max)  ← vira FILESTREAM ao rodar este script
--   • ArquMomento     datetime2 DEFAULT GETDATE()
--   • ArquAutor       nvarchar(150)
--   • ArquAtivo       bit DEFAULT 1
--   • ArquOrigem      nvarchar(50)
--
-- Este script segue o padrão do banco PrefeituraDigital (filegroup
-- FG_Arquivos_Stream, arquivo lógico fg_TurismoEstancia). Executar com
-- privilégios de sysadmin, com o FILESTREAM habilitado na instância.
-- É um script de execução ÚNICA (a conversão do item 3 não é reversível
-- sem copiar os bytes de volta). As etapas 1 e 2 são guardadas para poder
-- ser reexecutadas com segurança.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1) Habilitar FILESTREAM na instância (uma única vez por servidor)
--    Valores: 0 = desabilitado | 1 = somente T-SQL | 2 = T-SQL + acesso de
--    streaming do Win32 (recomendado). Pode exigir restart da instância.
-- ----------------------------------------------------------------------------
EXEC sp_configure 'filestream_access_level', 2;
RECONFIGURE;
GO

-- Confirma o nível de acesso atual
SELECT * FROM sys.configurations WHERE name = 'filestream_access_level';
GO

-- ----------------------------------------------------------------------------
-- 2) Criar o filegroup FILESTREAM e o arquivo de dados
--    O diretório 'D:\MSSQL\TurismoEstancia_FS' PRECISA existir e estar vazio
--    (criar na unidade D:, como no padrão PrefeituraDigital).
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.filegroups WHERE name = 'FG_Arquivos_Stream')
BEGIN
    ALTER DATABASE [TurismoEstanciaDb]
        ADD FILEGROUP [FG_Arquivos_Stream] CONTAINS FILESTREAM;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_files WHERE name = 'fg_TurismoEstancia')
BEGIN
    ALTER DATABASE [TurismoEstanciaDb]
        ADD FILE
        (
            NAME       = fg_TurismoEstancia,
            FILENAME   = N'D:\MSSQL\TurismoEstancia_FS'
        )
        TO FILEGROUP [FG_Arquivos_Stream];
END
GO

-- ----------------------------------------------------------------------------
-- 3) Converter ArquBytes para varbinary(max) FILESTREAM
--    A tabela já possui o ROWGUIDCOL (ArquUID), requisito do SQL Server.
--    A conversão copia os bytes atuais e troca a coluna em uma transação.
-- ----------------------------------------------------------------------------
BEGIN TRANSACTION;

    -- Coluna nova FILESTREAM (vai para o filegroup FG_Arquivos_Stream)
    ALTER TABLE [dbo].[Arquivos]
        ADD [ArquBytesFilestream] varbinary(max) FILESTREAM;

    -- Migra o binário já gravado (imagens/vídeos do seed e do CMS)
    UPDATE [dbo].[Arquivos]
       SET [ArquBytesFilestream] = [ArquBytes]
     WHERE [ArquBytes] IS NOT NULL;

    -- Remove a coluna antiga (dados de linha) e renomeia a nova
    ALTER TABLE [dbo].[Arquivos] DROP COLUMN [ArquBytes];
    EXEC sp_rename N'[dbo].[Arquivos].[ArquBytesFilestream]', N'ArquBytes', N'COLUMN';

COMMIT TRANSACTION;
GO

-- ----------------------------------------------------------------------------
-- 4) Verificação
-- ----------------------------------------------------------------------------
SELECT t.name AS Tabela,
       fg.name AS Filegroup,
       c.name AS Coluna,
       c.is_rowguidcol,
       c.is_filestream
  FROM sys.tables t
  JOIN sys.columns c ON c.object_id = t.object_id
  LEFT JOIN sys.filegroups fg ON fg.data_space_id = c.filestream_data_space_id
 WHERE t.name = 'Arquivos'
 ORDER BY c.column_id;
GO
