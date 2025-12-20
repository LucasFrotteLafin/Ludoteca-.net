using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ludoteca.Models;

public class Membro
{
    // Constantes para validação
    private const int CODIGO_MAXIMO = 999999;
    private const string PADRAO_EMAIL = @"^[a-zA-Z0-9._]+@[a-zA-Z0-9._]+\.[a-zA-Z]{2,}$";
    private const string PADRAO_TELEFONE = @"^[1-9]{2}[0-9]{8,9}$";
    private const int IDADE_MAXIMA = 120;

    // Propriedades públicas
    public int CodigoMembro { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Telefone { get; private set; }
    public DateTime DataNascimento { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public int Idade => CalcularIdade();

    [JsonConstructor]
    public Membro(int codigoMembro, string nome, string email, string telefone, DateTime dataNascimento, DateTime dataCadastro = default)
    {
        // Se dataCadastro não foi informada, usa data atual
        if (dataCadastro == default)
            dataCadastro = DateTime.Now;
            
        ValidarDados(codigoMembro, nome, email, telefone, dataNascimento, dataCadastro);
        
        // Definir propriedades do membro
        CodigoMembro = codigoMembro;
        Nome = nome ?? string.Empty;
        Email = email ?? string.Empty;
        Telefone = telefone ?? string.Empty;
        DataNascimento = dataNascimento;
        DataCadastro = dataCadastro;
    }

    // Método centralizado de validação
    private static void ValidarDados(int codigo, string nome, string email, string telefone, DateTime dataNascimento, DateTime dataCadastro)
    {
        ValidarCodigo(codigo);
        ValidarNome(nome);
        ValidarEmail(email);
        ValidarTelefone(telefone);
        ValidarDataNascimento(dataNascimento);
        ValidarDataCadastro(dataCadastro);
    }

    private static void ValidarCodigo(int codigo)
    {
        if (codigo <= 0 || codigo > CODIGO_MAXIMO)
            throw new ArgumentException($"Código deve ser entre 1 e {CODIGO_MAXIMO}", nameof(codigo));
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório", nameof(nome));
        
        // Verificar se contém apenas letras e espaços
        if (!Regex.IsMatch(nome, @"^[a-zA-ZÀ-ÿ\s]+$"))
            throw new ArgumentException("Nome deve conter apenas letras e espaços", nameof(nome));
    }

    private static void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, PADRAO_EMAIL))
            throw new ArgumentException("Email inválido", nameof(email));
    }

    private static void ValidarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone é obrigatório", nameof(telefone));
        
        // Verificar se contém apenas números
        if (!Regex.IsMatch(telefone, @"^[0-9]+$"))
            throw new ArgumentException("Telefone deve conter apenas números", nameof(telefone));
        
        // Verificar formato brasileiro
        if (!Regex.IsMatch(telefone, @"^[1-9]{2}[0-9]{8,9}$"))
            throw new ArgumentException("Telefone deve ter formato: DDNNNNNNNN ou DDNNNNNNNNN (ex: 1187654321 ou 11987654321)", nameof(telefone));
    }

    private static void ValidarDataNascimento(DateTime data)
    {
        if (data > DateTime.Now)
            throw new ArgumentException("Data de nascimento não pode ser futura", nameof(data));
        if (data < DateTime.Now.AddYears(-IDADE_MAXIMA))
            throw new ArgumentException($"Idade não pode ser superior a {IDADE_MAXIMA} anos", nameof(data));
    }

    private static void ValidarDataCadastro(DateTime data)
    {
        if (data > DateTime.Now)
            throw new ArgumentException("Data de cadastro não pode ser futura", nameof(data));
    }

    private int CalcularIdade()
    {
        var hoje = DateTime.Now;
        var idade = hoje.Year - DataNascimento.Year;
        
        // Ajusta se ainda não fez aniversário este ano
        if (hoje.Month < DataNascimento.Month || 
            (hoje.Month == DataNascimento.Month && hoje.Day < DataNascimento.Day))
            idade--;
            
        return idade;
    }

    public bool PodeAlugarJogo(int idadeMinimaJogo) => Idade >= idadeMinimaJogo;

    public override string ToString() =>
        $"{Nome} | {Email} | {Telefone} | {Idade} anos | Cadastro: {DataCadastro:dd/MM/yyyy}";
}