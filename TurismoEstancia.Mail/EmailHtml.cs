using System.Net;
using System.Text;

namespace TurismoEstancia.Mail;

/// <summary>
/// Monta o HTML do e-mail de marketing da newsletter: corpo em texto puro
/// vira HTML seguro (escapado) com quebras de linha, sobre um layout simples
/// com cores do portal e rodapé de descadastro (boas práticas LGPD).
/// </summary>
public static class EmailHtml
{
    public static string Marketing(string assunto, string corpoTexto)
    {
        var paragrafos = string.Join("</p><p style=\"margin:0 0 16px;font-size:15px;line-height:1.6;\">",
            (corpoTexto ?? string.Empty)
                .Split('\n')
                .Select(WebUtility.HtmlEncode)
                .Select(l => (l ?? string.Empty).Trim()));

        return $"""
        <div style="background:#0B1320;padding:32px 16px;font-family:Arial,Helvetica,sans-serif;">
          <div style="max-width:560px;margin:0 auto;background:#101C2C;border:1px solid #1E2F45;border-radius:14px;overflow:hidden;">
            <div style="padding:24px 28px;border-bottom:1px solid #1E2F45;background:linear-gradient(135deg,#F76400,#D63031);">
              <div style="font-size:13px;font-weight:bold;letter-spacing:1px;color:#FFFFFF;">DESCUBRA ESTÂNCIA</div>
              <div style="font-size:22px;font-weight:bold;color:#FFFFFF;margin-top:4px;">{WebUtility.HtmlEncode(assunto)}</div>
            </div>
            <div style="padding:28px;color:#DCE6F0;">
              <p style="margin:0 0 16px;font-size:15px;line-height:1.6;">{paragrafos}</p>
              <p style="margin:24px 0 0;font-size:12px;line-height:1.6;color:#8FA3B8;border-top:1px solid #1E2F45;padding-top:16px;">
                Você recebeu este e-mail porque se inscreveu na newsletter do portal de turismo de Estância/SE.<br />
                Para deixar de receber, responda este e-mail informando o descadastro.
              </p>
            </div>
          </div>
        </div>
        """;
    }
}
