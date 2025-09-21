using System;
using System.Text.RegularExpressions;

namespace Ludoteca.models;

public class Membro
{
    public int Senha { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Telefone { get; private set; }
    public DateTime DataCadastro { get; private set; }


    public Membro(int senha, string nome, string email, string telefone, DateTime dataCadastro)
    {
        if (senha <= 0 || senha > 999999)
            throw new ArgumentException("A senha deve ser um número positivo e ter no máximo 6 dígitos", nameof(senha));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome não pode ser vazio", nameof(nome));

        string padraoEmail = @"^[a-zA-Z0-9._]+@[a-zA-Z0-9._]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email, padraoEmail))
            throw new ArgumentException("Email inválido. O email não deve conter caracteres especiais, espaços e deve incluir @", nameof(email));

        string padraoTelefone = @"^[1-9]{2}[9]?[0-9]{8}$";
        if (string.IsNullOrWhiteSpace(telefone) || !Regex.IsMatch(telefone, padraoTelefone))
            throw new ArgumentException("Seu numero de telefone está inválido", nameof(telefone));

        if (dataCadastro > DateTime.Now)
            throw new ArgumentException("Data de cadastro não pode ser futura.", nameof(dataCadastro));

        Senha = senha;
        Nome = nome;
        Email = email;
        Telefone = telefone;
        DataCadastro = dataCadastro;
    }

    public Membro(int senha, string nome, string email, string telefone)
        : this(senha, nome, email, telefone, DateTime.Now)
    {
    }

    public override string ToString()
    {
        return $"{Nome} | {Email} | {Telefone} | Cadastro : {DataCadastro:dd/MM/yyyy}";
    }
}

