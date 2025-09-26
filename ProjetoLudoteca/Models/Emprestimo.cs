using System;
using System.Text.Json.Serialization;

namespace Ludoteca.Models;

public class Emprestimo
{
    private const int DIAS_MAXIMO_EMPRESTIMO = 7;

    public int Id { get; private set; }  // [AV1-2]
    public int IdJogo { get; private set; }  // [AV1-2]
    public int CodigoMembro { get; private set; }  // [AV1-2]
    public DateTime DataEmprestimo { get; private set; }  // [AV1-2]
    public DateTime DataDevolucao { get; private set; }  // [AV1-2]
    public bool Ativo { get; private set; }  // [AV1-2]
    public bool MultaPaga { get; private set; } = false;
    [JsonIgnore]
    public decimal ValorMulta => DiasAtraso * 2.50m;
    [JsonIgnore]
    public int DiasAtraso => DateTime.Now > DataDevolucao ? (DateTime.Now.Date - DataDevolucao.Date).Days : 0;
    public string MetodoPagamento { get; private set; } = "";

    public Emprestimo() { }

    public Emprestimo(int idJogo, int codigoMembro, int diasEmprestimo)
    {
        ValidarIdPositivo(idJogo, nameof(idJogo), "ID do jogo");
        ValidarIdPositivo(codigoMembro, nameof(codigoMembro), "Código do membro");

        DateTime agora = DateTime.Now;
        
        IdJogo = idJogo;
        CodigoMembro = codigoMembro;
        DataEmprestimo = agora;
        DataDevolucao = agora.AddDays(diasEmprestimo);
    }

    private static void ValidarIdPositivo(int valor, string nomeParametro, string tipoEntidade)
    {
        if (valor <= 0)
            throw new ArgumentException($"{tipoEntidade} deve ser maior que zero", nomeParametro);
    }

    public bool EstaAtrasado() =>
        DateTime.Now > DataDevolucao;

    public void RegistrarPagamentoMulta(string metodo)
    {
        MultaPaga = true;
        MetodoPagamento = metodo;
    }

    public override string ToString() =>
        $"Jogo: {IdJogo} | Membro: {CodigoMembro} | Empréstimo: {DataEmprestimo:dd/MM/yyyy} | Devolução: {DataDevolucao:dd/MM/yyyy}";
}