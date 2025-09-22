using System;
using System.Text.Json.Serialization;

namespace Ludoteca.Models;

public class Emprestimo
{
    private const int DIAS_MAXIMO_EMPRESTIMO = 7;

    public int Id { get; set; }  // [AV1-2]
    public int IdJogo { get; set; }  // [AV1-2]
    public int CodigoMembro { get; set; }  // [AV1-2]
    public DateTime DataEmprestimo { get; set; }  // [AV1-2]
    public DateTime DataDevolucao { get; set; }  // [AV1-2]
    public bool Ativo { get; set; }  // [AV1-2]

    public Emprestimo() { }

    public Emprestimo(int id, int idJogo, int codigoMembro, int diasEmprestimo)
    {
        ValidarId(id);
        ValidarIdJogo(idJogo);
        ValidarCodigoMembro(codigoMembro);
        ValidarDiasEmprestimo(diasEmprestimo);

        DateTime agora = DateTime.Now;
        
        Id = id;
        IdJogo = idJogo;
        CodigoMembro = codigoMembro;
        DataEmprestimo = agora;
        DataDevolucao = agora.AddDays(diasEmprestimo);
        Ativo = true;
    }

    private static void ValidarId(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID deve ser maior que zero", nameof(id));
    }

    private static void ValidarIdJogo(int idJogo)
    {
        if (idJogo <= 0)
            throw new ArgumentException("ID do jogo deve ser maior que zero", nameof(idJogo));
    }

    private static void ValidarCodigoMembro(int codigoMembro)
    {
        if (codigoMembro <= 0)
            throw new ArgumentException("Código do membro deve ser maior que zero", nameof(codigoMembro));
    }

    private static void ValidarDiasEmprestimo(int dias)
    {
        if (dias <= 0 || dias > DIAS_MAXIMO_EMPRESTIMO)
            throw new ArgumentException($"Dias deve ser entre 1 e {DIAS_MAXIMO_EMPRESTIMO}", nameof(dias));
    }

    public void Devolver()
    {
        if (!Ativo)
            throw new InvalidOperationException("Empréstimo já foi devolvido");
        Ativo = false;
    }

    public bool EstaAtrasado(DateTime? dataAtual = null) =>
        Ativo && (dataAtual ?? DateTime.Now) > DataDevolucao;

    public override string ToString() =>
        $"ID: {Id} | Jogo: {IdJogo} | Membro: {CodigoMembro} | Empréstimo: {DataEmprestimo:dd/MM/yyyy} | Devolução: {DataDevolucao:dd/MM/yyyy} | Status: {(Ativo ? "Ativo" : "Devolvido")}";
}