using System;
using System.Text.Json.Serialization;

namespace Ludoteca.Models;

public class Jogo
{
    private const int RESTRICAO_DE_IDADE = 18;
    
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Categoria { get; private set; }
    public int IdadeMinima { get; private set; }
    public bool EstaEmprestado { get; private set; }

    public Jogo()
    {
        Nome = Categoria = string.Empty;
    }

    [JsonConstructor]
    public Jogo(int id, string nome, string categoria, int idadeMinima, bool estaEmprestado = false)
    {
        ValidarId(id);
        ValidarNome(nome);
        ValidarCategoria(categoria);
        ValidarIdadeMinima(idadeMinima);

        
        Id = id;
        Nome = nome;
        Categoria = categoria;
        IdadeMinima = idadeMinima;
        EstaEmprestado = estaEmprestado;
    }

    private static void ValidarId(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID deve ser maior que zero", nameof(id));
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
    }

    private static void ValidarCategoria(string categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria))
            throw new ArgumentException("Categoria não pode ser vazia", nameof(categoria));
    }

    private static void ValidarIdadeMinima(int idade)
    {
        if (idade < 0 || idade > RESTRICAO_DE_IDADE)
            throw new ArgumentException($"Idade deve estar entre 0 e {RESTRICAO_DE_IDADE} anos", nameof(idade));
    }

    public void MarcarComoEmprestado()
    {
        if (EstaEmprestado)
            throw new InvalidOperationException($"Jogo '{Nome}' já está emprestado");
        // Marcar como emprestado
        EstaEmprestado = true;
    }

    public void MarcarComoDevolvido()
    {
        // Marcar como disponível
        EstaEmprestado = false;
    }

    public override string ToString() =>
        $"[{Id:D3}] {Nome} | {Categoria} | {IdadeMinima}+ anos | {(EstaEmprestado ? "EMPRESTADO" : "DISPONÍVEL")}";
}