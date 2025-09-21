using System;

namespace Ludoteca.models

public class Jogo
{
    public string Nome { get; private set; }
    public string Categoria { get; private set; }
    public int IdadeMinima { get; private set; }
    public int Id { get; private set; }
    public bool Situacao { get; private set; }

    public Jogo(string nome, string categoria, int idadeMinima, int id, bool situacao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome não pode ser vazio ", nameof(nome));

        if (string.IsNullOrWhiteSpace(categoria))
            throw new ArgumentException("Nome de categoria inválida ", nameof(categoria));

        if (idadeMinima > 12 )
            throw new ArgumentException("Idade mínima deve ser maior 12 ", nameof(idadeMinima));

        this.nome = nome;
        this.categoria = categoria;
        this.idadeMinima = idadeMinima;
        this.id = id; //começar a contar desde o 000 sempre somando +1 a cada jogo novo (usar .PadLeft)
        this.situacao = situacao;
    }

    public override string ToString()
    {
        return $"{Id} | {Nome} | {Categoria} | {IdadeMinima} | {Situacao}"
    }

}

