using System;
using System.Text.Json.Serialization;

namespace Ludoteca.Models;

public class Emprestimo
{
    private const int DIAS_MAXIMO_EMPRESTIMO = 7;

    public int Id { get; set; }
    public int IdJogo { get; private set; }
    public int CodigoMembro { get; private set; }
    public DateTime DataEmprestimo { get; private set; }
    public DateTime DataDevolucao { get; private set; }
    public DateTime? DataDevolucaoReal { get; private set; }
    public bool Ativo { get; private set; } = true;
    public bool MultaPaga { get; private set; } = false;
    [JsonIgnore]
    public decimal ValorMulta => Ativo ? DiasAtraso * 2.50m : 0;
    [JsonIgnore]
    public int DiasAtraso => Ativo && DateTime.Now > DataDevolucao ? (DateTime.Now.Date - DataDevolucao.Date).Days : 0;
    public string MetodoPagamento { get; private set; } = "";

    public Emprestimo() { }

    public Emprestimo(int idJogo, int codigoMembro, int diasEmprestimo)
    {
        ValidarIdPositivo(idJogo, nameof(idJogo), "ID do jogo");
        ValidarIdPositivo(codigoMembro, nameof(codigoMembro), "Código do membro");

        DateTime agora = DateTime.Now;
        
        // Definir dados do empréstimo
        IdJogo = idJogo;
        CodigoMembro = codigoMembro;
        DataEmprestimo = agora;
        DataDevolucao = agora.AddDays(diasEmprestimo);
        Ativo = true;
    }

    private static void ValidarIdPositivo(int valor, string nomeParametro, string tipoEntidade)
    {
        if (valor <= 0)
            throw new ArgumentException($"{tipoEntidade} deve ser maior que zero", nomeParametro);
    }

    public bool EstaAtrasado() =>
        Ativo && DateTime.Now > DataDevolucao;

    public void RegistrarDevolucao()
    {
        // Registrar data real de devolução
        DataDevolucaoReal = DateTime.Now;
        Ativo = false;
    }

    public void RegistrarPagamentoMulta(string metodo)
    {
        // Marcar multa como paga
        MultaPaga = true;
        MetodoPagamento = metodo;
    }

    public override string ToString() =>
        $"Jogo: {IdJogo} | Membro: {CodigoMembro} | Empréstimo: {DataEmprestimo:dd/MM/yyyy} | Devolução: {DataDevolucao:dd/MM/yyyy}";
}