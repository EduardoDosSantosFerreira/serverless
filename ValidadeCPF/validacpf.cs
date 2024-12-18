using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ValidaCpf
{
    public static class validacpf
    {
        [FunctionName("validacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando a validação do CPF.");

            // Lê o corpo da requisição HTTP e converte para string
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // Deserializa o JSON recebido para um objeto dinâmico
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
            {
                // Retorna um erro se o CPF não for informado
                return new BadRequestObjectResult("Por favor, informe o CPF.");
            }
            // Extrai o CPF do objeto deserializado
            string cpf = data?.cpf;

            // Chama o método para validar o CPF
            if (ValidaCPF(cpf) == false)
            {
                // Retorna um erro se o CPF for inválido
                return new BadRequestObjectResult("CPF inválido.");
            }

            // Mensagem de sucesso se o CPF for válido
            var responseMessage = "CPF válido.";

            // Retorna a resposta de sucesso
            return new OkObjectResult(responseMessage);
        }

        // Método para validar o CPF
        public static bool ValidaCPF(string cpf)
        {
            // Verifica se o CPF é nulo ou vazio
            if (string.IsNullOrEmpty(cpf))
                return false;

            // Remove pontos e traços do CPF
            cpf = cpf.Trim().Replace(".", "").Replace("-", "");

            // Verifica se o CPF tem exatamente 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos do CPF são iguais, o que é inválido
            if (new string(cpf[0], cpf.Length) == cpf)
                return false;

            // Arrays de multiplicadores para cálculo dos dígitos verificadores
            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            // Extrai os primeiros 9 dígitos do CPF
            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            // Calcula o primeiro dígito verificador
            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            // Adiciona o primeiro dígito verificador ao CPF temporário
            tempCpf += digito1;
            soma = 0;

            // Calcula o segundo dígito verificador
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            // Verifica se os dígitos calculados são iguais aos dígitos do CPF
            return cpf.EndsWith(digito1.ToString() + digito2.ToString());
        }
    }
}