using System;

namespace Ludoteca.Models;

public class Emprestimo
{
    public int Id { get; private set; }
    public int IdJogo { get; private set; }
    public int CodigoMembro { get; private set; }
    public DateTime DataEmprestimo { get; private set; }
    public DateTime DataDevolucao { get; private set; }
    public bool Ativo { get; private set; }

    public Emprestimo(int id, int idJogo, int codigoMembro, int diasEmprestimo)
    {
        if (id <= 0)
            throw new ArgumentException("ID deve ser maior que zero", nameof(id));

        if (idJogo <= 0)
            throw new ArgumentException("ID do jogo deve ser maior que zero", nameof(idJogo));

        if (codigoMembro <= 0)
            throw new ArgumentException("Código do membro deve ser maior que zero", nameof(codigoMembro));

        if (diasEmprestimo <= 0 || diasEmprestimo > 7)
            throw new ArgumentException("Dias de empréstimo deve ser entre 1 e 7 , com aplicação de multa de 2,50 por dia caso ultrapasse", nameof(diasEmprestimo));

        DateTime agora = DateTime.Now;
        
        Id = id;
        IdJogo = idJogo;
        CodigoMembro = codigoMembro;
        DataEmprestimo = agora;
        DataDevolucao = agora.AddDays(diasEmprestimo);
        Ativo = true;
    }

    public void Devolver()
    {
        if (!Ativo)
            throw new InvalidOperationException("Empréstimo já foi devolvido");

        Ativo = false;
    }

    public bool EstaAtrasado(DateTime? dataAtual = null)
    {
        DateTime agora = dataAtual ?? DateTime.Now;
        return Ativo && agora > DataDevolucao;
    }

    public override string ToString()
    {
        string status = Ativo ? "Ativo" : "Devolvido";
        return $"ID: {Id} | Jogo: {IdJogo} | Membro: {CodigoMembro} | Empréstimo: {DataEmprestimo:dd/MM/yyyy} | Devolução: {DataDevolucao:dd/MM/yyyy} | Status: {status}";
    }
}