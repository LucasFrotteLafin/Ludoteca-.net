using System;

namespace Ludoteca.models;

public class Emprestimo
{
    public int Id { get; private set; }
    public int IdJogo { get; private set; }
    public int SenhaMembro { get; private set; }
    public DateTime DataEmprestimo { get; private set; }
    public DateTime DataDevolucao { get; private set; }
    public bool Ativo { get; private set; }

    public Emprestimo(int id, int idJogo, int senhaMembro, int diasEmprestimo)
    {
        if (id <= 0)
            throw new ArgumentException("ID deve ser maior que zero", nameof(id));

        if (idJogo <= 0)
            throw new ArgumentException("ID do jogo deve ser maior que zero", nameof(idJogo));

        if (senhaMembro <= 0)
            throw new ArgumentException("Senha do membro deve ser maior que zero", nameof(senhaMembro));

        if (diasEmprestimo <= 0 || diasEmprestimo > 30)
            throw new ArgumentException("Dias de empréstimo deve ser entre 1 e 30", nameof(diasEmprestimo));

        Id = id;
        IdJogo = idJogo;
        SenhaMembro = senhaMembro;
        DataEmprestimo = DateTime.Now;
        DataDevolucao = DateTime.Now.AddDays(diasEmprestimo);
        Ativo = true;
    }

    public void Devolver()
    {
        if (!Ativo)
            throw new InvalidOperationException("Empréstimo já foi devolvido");

        Ativo = false;
    }

    public bool EstaAtrasado()
    {
        return Ativo && DateTime.Now > DataDevolucao;
    }

    public override string ToString()
    {
        string status = Ativo ? "Ativo" : "Devolvido";
        return $"ID: {Id} | Jogo: {IdJogo} | Membro: {SenhaMembro} | Empréstimo: {DataEmprestimo:dd/MM/yyyy} | Devolução: {DataDevolucao:dd/MM/yyyy} | Status: {status}";
    }
}