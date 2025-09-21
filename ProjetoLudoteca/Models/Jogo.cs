using System;
using System.Text.Json.Serialization;

namespace Ludoteca.Models;

public class Jogo
{
    public const int IDADE_MAXIMA_PERMITIDA = 18;
    
    // Encapsulamento com private set
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Categoria { get; private set; }
    public int IdadeMinima { get; private set; }
    public bool EstaEmprestado { get; private set; }

    // Construtor com validações
    [JsonConstructor]
    public Jogo(int id, string nome, string categoria, int idadeMinima, bool estaEmprestado = false)
    {
        if (id <= 0)
            throw new ArgumentException("ID deve ser maior que zero.", nameof(id));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));

        if (string.IsNullOrWhiteSpace(categoria))
            throw new ArgumentException("Categoria não pode ser vazia.", nameof(categoria));

        if (idadeMinima < 0 || idadeMinima > IDADE_MAXIMA_PERMITIDA)
            throw new ArgumentException($"Idade mínima deve estar entre 0 e {IDADE_MAXIMA_PERMITIDA} anos.", nameof(idadeMinima));

        Id = id;
        Nome = nome;
        Categoria = categoria;
        IdadeMinima = idadeMinima;
        EstaEmprestado = estaEmprestado;
    }

    public void MarcarComoEmprestado()
    {
        if (EstaEmprestado)
            throw new InvalidOperationException($"Jogo '{Nome}' já está emprestado.");

        EstaEmprestado = true;
    }

    public void MarcarComoDevolvido()
    {
        if (!EstaEmprestado)
            throw new InvalidOperationException($"Jogo '{Nome}' não está emprestado.");

        EstaEmprestado = false;
    }

    public override string ToString()
    {
        string status = EstaEmprestado ? "EMPRESTADO" : "DISPONÍVEL";
        return $"[{Id:D3}] {Nome} | {Categoria} | {IdadeMinima}+ anos | {status}";
    }
}