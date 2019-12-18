using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Akumu.Antigate;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;

namespace DecodificadorCaptcha.Business
{
    public class DecodificadorBusiness
    {
        private readonly AntiCaptcha _antiGate;
        private string _base64 { get; set; }
        private string _pathCaptcha { get; set; }

        public DecodificadorBusiness(string Base64)
        {
            _base64 = Base64;

            //Configurando instância do antigate com o token de acesso.
            _antiGate = new AntiCaptcha(ConfigurationManager.AppSettings["TokenAntigate"])
            {
                CheckDelay = 5000,
                CheckRetryCount = 20,
                SlotRetry = 5,
                SlotRetryDelay = 800
            };

            //Gerando URL da imagem cache..
            GetPathCaptcha();
        }

        private void GetPathCaptcha()
        {
            try
            {
                //Nome da imagem.
                string nomeImagem = $@"\{Guid.NewGuid().ToString()}.png";
                string caminhoImagem = $@"{HttpContext.Current.Server.MapPath("~")}\captcha";

                //Verificando se existe o caminho
                if (!Directory.Exists(caminhoImagem))
                    Directory.CreateDirectory(caminhoImagem);
                
                //Retornando caminho da imagem.
                _pathCaptcha = $@"{caminhoImagem}{nomeImagem}";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        private async Task<bool> CreateImagem()
        {
            try
            {
                if (!string.IsNullOrEmpty(_pathCaptcha) && !File.Exists(_pathCaptcha))
                {
                    //Convertendo Base64 para byte array.
                    byte[] imageBytes = Convert.FromBase64String(_base64);

                    //Salvando a matriz de bytes como arquivo de imagem cache.
                    File.WriteAllBytes(_pathCaptcha, imageBytes);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        private void DeleteImage()
        {
            try
            {
                //Deletando imagem cache.
                if (!string.IsNullOrEmpty(_pathCaptcha) && File.Exists(_pathCaptcha))
                    File.Delete(_pathCaptcha);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        public string Init()
        {
            try
            {
                //Verificando se a URL cache foi criada.
                if (!string.IsNullOrEmpty(_pathCaptcha))
                {
                    //Criando imagem para enviar ao AntiGate.
                    var create = CreateImagem().Result;

                    //Verificando resultado do create da imagme.
                    if (create)
                    {
                        //Recebendo o retorno do Antigame.
                        string response = _antiGate.GetAnswer(_pathCaptcha);

                        //Verificando resultado do Antigate.
                        if (!string.IsNullOrEmpty(response))
                            return response;
                    }
                }

                return ConfigurationManager.AppSettings["MensagemErro"];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            finally
            {
                //Deletando imagem de cache..
                DeleteImage();
            }
        }
    }
}