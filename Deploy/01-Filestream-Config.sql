-- ============================================================================
-- TurismoEstancia — Configuração do FILESTREAM (executar no servidor de produção)
-- ----------------------------------------------------------------------------
-- A tabela [Arquivos] nasce preparada para FILESTREAM pelo padrão adotado:
--   • ArquId          bigint identity (PK, em fg_dados)
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
-- Banco real: **portalTurismo** (sqlserver01.estancia.local). Os filegroups já
-- estão configurados no servidor:
--   • fg_dados            — filegroup de DADOS (default)  → dados da tabela
--   • fg_portalTurismo    — filegroup FILESTREAM (default) → binários
--
-- ⚠️ PRÉ-REQUISITO (erro 5505): a coluna ROWGUIDCOL (ArquUID) precisa de uma
-- **constraint UNIQUE** (um índice único puro NÃO satisfaz o SQL Server).
-- O EF Core não modela isso — a constraint entra via SQL, como o ROWGUIDCOL.
--
-- ⚠️ A conversão é de execução ÚNICA e usa batches separados (GO): o SQL Server
-- compila cada batch antes de executar, então não se pode referenciar no mesmo
-- batch uma coluna criada nele. Rodar com privilégios de sysadmin e FILESTREAM
-- habilitado na instância (sp_configure 'filestream_access_level', 2).
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 0) Pré-requisito: constraint UNIQUE no ROWGUIDCOL (ArquUID)
--    Se ainda não existir (a tabela recém-migrada pelo EF não tem):
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
               WHERE parent_object_id = OBJECT_ID('dbo.Arquivos')
                 AND name = 'UQ_Arquivos_ArquUID')
BEGIN
    ALTER TABLE dbo.Arquivos ADD CONSTRAINT [UQ_Arquivos_ArquUID] UNIQUE NONCLUSTERED (ArquUID);
END
GO

-- ----------------------------------------------------------------------------
-- 1) Converter ArquBytes para varbinary(max) FILESTREAM
--    A coluna nova vai para o filegroup FILESTREAM padrão (fg_portalTurismo);
--    o UPDATE copia o binário já gravado (vazio em banco recém-migrado).
-- ----------------------------------------------------------------------------
ALTER TABLE dbo.Arquivos ADD [ArquBytesFilestream] varbinary(max) FILESTREAM;
GO

UPDATE dbo.Arquivos SET [ArquBytesFilestream] = [ArquBytes] WHERE [ArquBytes] IS NOT NULL;
GO

ALTER TABLE dbo.Arquivos DROP COLUMN [ArquBytes];
EXEC sp_rename N'dbo.Arquivos.ArquBytesFilestream', N'ArquBytes', N'COLUMN';
GO

ALTER TABLE dbo.Arquivos ALTER COLUMN [ArquBytes] varbinary(max) FILESTREAM NOT NULL;
GO

-- ----------------------------------------------------------------------------
-- 2) Verificação — ArquBytes deve sair is_filestream = 1 e ArquUID is_rowguidcol = 1
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
