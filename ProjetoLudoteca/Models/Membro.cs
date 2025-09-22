using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ludoteca.Models;

public class Membro
{
    private const int CODIGO_MAXIMO = 999999;
    private const string PADRAO_EMAIL = @"^[a-zA-Z0-9._]+@[a-zA-Z0-9._]+\.[a-zA-Z]{2,}$";
    private const string PADRAO_TELEFONE = @"^[1-9]{2}[9]?[0-9]{8}$";

    public int CodigoMembro { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Telefone { get; private set; }
    public DateTime DataNascimento { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public int Idade => CalcularIdade();

    public Membro() 
    {
        Nome = Email = Telefone = string.Empty;
    }

    [JsonConstructor]
    public Membro(int codigoMembro, string nome, string email, string telefone, DateTime dataNascimento, DateTime dataCadastro)
    {
        ValidarCodigo(codigoMembro);
        ValidarNome(nome);
        ValidarEmail(email);
        ValidarTelefone(telefone);
        ValidarDataNascimento(dataNascimento);
        ValidarDataCadastro(dataCadastro);

        CodigoMembro = codigoMembro;
        Nome = nome;
        Email = email;
        Telefone = telefone;
        DataNascimento = dataNascimento;
        DataCadastro = dataCadastro;
    }

    public Membro(int codigoMembro, string nome, string email, string telefone, DateTime dataNascimento)
        : this(codigoMembro, nome, email, telefone, dataNascimento, DateTime.Now) { }

    private static void ValidarCodigo(int codigo)
    {
        if (codigo <= 0 || codigo > CODIGO_MAXIMO)
            throw new ArgumentException($"Código deve ser entre 1 e {CODIGO_MAXIMO}", nameof(codigo));
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
    }

    private static void ValidarEmail(string email)
    {
        if (!Regex.IsMatch(email, PADRAO_EMAIL))
            throw new ArgumentException("Email inválido", nameof(email));
    }

    private static void ValidarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone) || !Regex.IsMatch(telefone, PADRAO_TELEFONE))
            throw new ArgumentException("Telefone inválido", nameof(telefone));
    }

    private static void ValidarDataNascimento(DateTime data)
    {
        if (data > DateTime.Now)
            throw new ArgumentException("Data de nascimento não pode ser futura", nameof(data));
        if (data < DateTime.Now.AddYears(-120))
            throw new ArgumentException("Data de nascimento inválida", nameof(data));
    }

    private static void ValidarDataCadastro(DateTime data)
    {
        if (data > DateTime.Now)
            throw new ArgumentException("Data não pode ser futura", nameof(data));
    }

    private int CalcularIdade()
    {
        var hoje = DateTime.Now;
        var idade = hoje.Year - DataNascimento.Year;
        if (hoje < DataNascimento.AddYears(idade))
            idade--;
        return idade;
    }

    public bool PodeAlugarJogo(int idadeMinimaJogo) => Idade >= idadeMinimaJogo;

    public override string ToString() =>
        $"{Nome} | {Email} | {Telefone} | {Idade} anos | Cadastro: {DataCadastro:dd/MM/yyyy}";
}