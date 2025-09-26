using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ludoteca.Models;

namespace Ludoteca.Services;

public class BibliotecaJogos
{
    private List<Jogo> jogos;
    private List<Membro> membros;
    private List<Emprestimo> emprestimos;
    private int proximoIdJogo;
    private readonly string caminhoArquivo;

    public BibliotecaJogos(string? caminhoPersonalizado = null)
    {
        jogos = new List<Jogo>();
        membros = new List<Membro>();
        emprestimos = new List<Emprestimo>();
        proximoIdJogo = 1;
        caminhoArquivo = caminhoPersonalizado ?? "Data/biblioteca.json";

        try
        {
            VerificarPasta();
            CarregarDados();
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - Inicialização: {ex.Message} | StackTrace: {ex.StackTrace}\n");
        }
    }

    private void VerificarPasta()
    {
        string pasta = "Data";
        if (!Directory.Exists(pasta))
        {
            Directory.CreateDirectory(pasta);
        }
    }

    public void AdicionarJogo(string nome, string categoria, int idadeMinima)
    {
        if (jogos == null)
            throw new InvalidOperationException("Lista de jogos não inicializada");

        bool jogoExiste = false;
        for (int i = 0; i < jogos.Count; i++)
        {
            if (jogos[i].Nome.ToLower() == nome.ToLower())
            {
                jogoExiste = true;
                break;
            }
        }

        if (jogoExiste)
        {
            throw new ArgumentException("Jogo já existe!");
        }

        Jogo novoJogo = new Jogo(proximoIdJogo, nome, categoria, idadeMinima);
        jogos.Add(novoJogo);
        if (proximoIdJogo >= int.MaxValue)
            throw new InvalidOperationException("Limite máximo de jogos atingido");
        proximoIdJogo++;
        SalvarDados();
    }

    public void AdicionarMembro(string nome, string email, string telefone, int codigoMembro, DateTime dataNascimento)
    {
        if (membros == null)
            throw new InvalidOperationException("Lista de membros não inicializada");

        bool emailExiste = false;
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].Email.ToLower() == email.ToLower())
            {
                emailExiste = true;
                break;
            }
        }

        if (emailExiste)
        {
            throw new ArgumentException("Email já existe!");
        }

        bool codigoExiste = false;
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].CodigoMembro == codigoMembro)
            {
                codigoExiste = true;
                break;
            }
        }

        if (codigoExiste)
        {
            throw new ArgumentException("Código do membro já existe!");
        }

        Membro novoMembro = new Membro(codigoMembro, nome, email, telefone, dataNascimento);
        membros.Add(novoMembro);
        SalvarDados();
    }

    public void RealizarEmprestimo(int idJogo, int codigoMembro)
    {
        if (jogos == null || membros == null || emprestimos == null)
            throw new InvalidOperationException("Listas não inicializadas");

        Jogo? jogo = null;
        for (int i = 0; i < jogos.Count; i++)
        {
            if (jogos[i].Id == idJogo)
            {
                jogo = jogos[i];
                break;
            }
        }

        if (jogo == null)
        {
            throw new ArgumentException("Jogo não encontrado!");
        }

        Membro? membro = null;
        for (int i = 0; i < membros.Count; i++)
        {
            if (membros[i].CodigoMembro == codigoMembro)
            {
                membro = membros[i];
                break;
            }
        }

        if (membro == null)
        {
            throw new ArgumentException($"Código do membro {codigoMembro} não encontrado!");
        }

        if (!membro.PodeAlugarJogo(jogo.IdadeMinima))
        {
            throw new InvalidOperationException($"Membro tem {membro.Idade} anos. Idade mínima para este jogo: {jogo.IdadeMinima} anos");
        }

        if (jogo.EstaEmprestado)
        {
            throw new InvalidOperationException("Jogo já está emprestado!");
        }

        jogo.MarcarComoEmprestado();
        Emprestimo novoEmprestimo = new Emprestimo(idJogo, codigoMembro, 7);
        emprestimos.Add(novoEmprestimo);
        SalvarDados();
    }

    public void RealizarDevolucao(int idJogo)
    {
        if (jogos == null || emprestimos == null)
            throw new InvalidOperationException("Listas não inicializadas");

        Jogo? jogo = null;
        for (int i = 0; i < jogos.Count; i++)
        {
            if (jogos[i].Id == idJogo)
            {
                jogo = jogos[i];
                break;
            }
        }

        if (jogo == null)
        {
            throw new ArgumentException("Jogo não encontrado!", nameof(idJogo));
        }

        if (!jogo.EstaEmprestado)
        {
            throw new InvalidOperationException("Jogo não está emprestado!");
        }
        
        // Buscar o empréstimo mais recente para este jogo
        Emprestimo? emprestimo = null;
        for (int i = emprestimos.Count - 1; i >= 0; i--)
        {
            if (emprestimos[i].IdJogo == idJogo)
            {
                emprestimo = emprestimos[i];
                break;
            }
        }

        if (emprestimo == null)
        {
            throw new InvalidOperationException("Empréstimo não encontrado para este jogo!");
        }

        if (emprestimo.EstaAtrasado())
        {
            Console.WriteLine($"Jogo está atrasado! Multa: R$ {emprestimo.ValorMulta:F2}");
            Console.Write("Deseja pagar a multa agora? (s/n): ");
            string resposta = Console.ReadLine()?.ToLower() ?? "";

            if (resposta == "s" || resposta == "sim")
            {
                Console.Write("Forma de pagamento (pix/dinheiro): ");
                string formaPagamento = Console.ReadLine() ?? "";

                if (formaPagamento.ToLower() == "pix" || formaPagamento.ToLower() == "dinheiro")
                {
                    emprestimo.RegistrarPagamentoMulta(formaPagamento.ToLower());
                    Console.WriteLine($"Multa de R$ {emprestimo.ValorMulta:F2} ({emprestimo.DiasAtraso} dias excedidos) paga via {formaPagamento}.");
                    
                    jogo.MarcarComoDevolvido();
                    SalvarDados();
                    Console.WriteLine("Jogo devolvido com sucesso!");
                }
                else
                {
                    Console.WriteLine("Forma de pagamento inválida. Multa não paga. Jogo não foi devolvido.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Multa não paga. Jogo não foi devolvido. Você deve pagar a multa antes de devolver.");
                return;
            }
        }
        else
        {
            jogo.MarcarComoDevolvido();
            SalvarDados();
            Console.WriteLine("Jogo devolvido com sucesso!");
        }
    }

    public void SalvarDados()
    {
        try
        {
            VerificarPasta();

            DadosBiblioteca dados = new DadosBiblioteca();
            dados.Jogos = jogos;
            dados.Membros = membros;
            dados.Emprestimos = emprestimos;
            dados.ProximoIdJogo = proximoIdJogo;

            JsonSerializerOptions opcoes = new JsonSerializerOptions();
            opcoes.WriteIndented = true;
            opcoes.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            string json = JsonSerializer.Serialize(dados, opcoes);
            File.WriteAllText(caminhoArquivo, json);
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - SalvarDados: {ex.Message} | StackTrace: {ex.StackTrace}\n");
            Console.WriteLine($"Erro ao salvar dados: {ex.Message}");
        }
    }

    public void CarregarDados()
    {
        try
        {
            // Limpar listas antes de carregar novos dados
            jogos.Clear();
            membros.Clear();
            emprestimos.Clear();
            
            if (!File.Exists(caminhoArquivo))
            {
                return;
            }

            string json = File.ReadAllText(caminhoArquivo);

            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            JsonSerializerOptions opcoes = new JsonSerializerOptions();
            opcoes.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            DadosBiblioteca? dados = JsonSerializer.Deserialize<DadosBiblioteca>(json, opcoes);

            if (dados != null)
            {
                jogos.AddRange(dados.Jogos ?? new List<Jogo>());
                membros.AddRange(dados.Membros ?? new List<Membro>());
                emprestimos.AddRange(dados.Emprestimos ?? new List<Emprestimo>());
                proximoIdJogo = dados.ProximoIdJogo;
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - CarregarDados: {ex.Message} | StackTrace: {ex.StackTrace}\n");
            Console.WriteLine($"Erro ao carregar dados: {ex.Message}");
        }
    }

    public decimal CalcularMulta(int idJogo)
    {
        CarregarDados();
        
        // Verificar se o jogo existe e está emprestado
        Jogo? jogo = null;
        for (int i = 0; i < jogos.Count; i++)
        {
            if (jogos[i].Id == idJogo)
            {
                jogo = jogos[i];
                break;
            }
        }
        
        if (jogo == null)
        {
            Console.WriteLine($"Jogo ID {idJogo} não encontrado");
            return 0;
        }
        
        if (!jogo.EstaEmprestado)
        {
            Console.WriteLine($"Jogo '{jogo.Nome}' não está emprestado");
            return 0;
        }
        
        // Buscar o empréstimo mais recente para este jogo
        Emprestimo? emprestimoAtual = null;
        for (int i = emprestimos.Count - 1; i >= 0; i--)
        {
            if (emprestimos[i].IdJogo == idJogo)
            {
                emprestimoAtual = emprestimos[i];
                break;
            }
        }
        
        if (emprestimoAtual == null)
        {
            Console.WriteLine($"Jogo '{jogo.Nome}' marcado como emprestado mas sem empréstimo registrado");
            return 0;
        }
        
        if (emprestimoAtual.EstaAtrasado())
        {
            Console.WriteLine($"Jogo '{jogo.Nome}' emprestado em {emprestimoAtual.DataEmprestimo:dd/MM/yyyy}, deveria ter sido devolvido em {emprestimoAtual.DataDevolucao:dd/MM/yyyy}. Atraso: {emprestimoAtual.DiasAtraso} dias. Multa: R$ {emprestimoAtual.ValorMulta:F2}");
            return emprestimoAtual.ValorMulta;
        }
        
        Console.WriteLine($"{jogo.Nome} está dentro do prazo registrado");
        return 0;
    }

    public void VerificarMulta()
    {
        CarregarDados();
        
        int idJogo;
        while (true)
        {
            Console.Write("ID do jogo para verificar multa: ");
            if (int.TryParse(Console.ReadLine(), out idJogo))
                break;
            Console.WriteLine("Por favor, digite um número válido.");
        }

        decimal multa = CalcularMulta(idJogo);
        Console.WriteLine($"\nResultado: R$ {multa:F2}");
    }

    public void CadastrarJogo()
    {
        try
        {
            Console.Write("Nome do jogo: ");
            string nome = Console.ReadLine() ?? "";
            Console.Write("Categoria: ");
            string categoria = Console.ReadLine() ?? "";
            Console.Write("Idade mínima: ");

            if (!int.TryParse(Console.ReadLine(), out int idade))
            {
                Console.WriteLine("Erro: Idade deve ser um número válido");
                return;
            }

            AdicionarJogo(nome, categoria, idade);
            Console.WriteLine("Jogo cadastrado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    public void CadastrarMembro()
    {
        try
        {
            Console.Write("Nome: ");
            string nome = Console.ReadLine() ?? "";
            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Telefone: ");
            string telefone = Console.ReadLine() ?? "";
            Console.Write("Código do membro (número): ");

            if (!int.TryParse(Console.ReadLine(), out int codigoMembro))
            {
                Console.WriteLine("Erro: Código deve ser um número válido");
                return;
            }

            Console.Write("Data de nascimento (dd/MM/yyyy): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataNascimento))
            {
                Console.WriteLine("Erro: Data deve estar no formato dd/MM/yyyy");
                return;
            }

            AdicionarMembro(nome, email, telefone, codigoMembro, dataNascimento);
            Console.WriteLine("Membro cadastrado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    public void ListarJogos()
    {
        if (jogos.Count == 0)
        {
            Console.WriteLine("Nenhum jogo cadastrado.");
            return;
        }

        Console.WriteLine("\n=== JOGOS CADASTRADOS ===");
        foreach (Jogo jogo in jogos)
        {
            Console.WriteLine(jogo.ToString());
        }
    }

    public void EmprestarJogo()
    {
        Console.Write("ID do jogo: ");
        if (!int.TryParse(Console.ReadLine(), out int idJogo))
        {
            throw new ArgumentException("ID do jogo deve ser um número válido");
        }

        Console.Write("Código do membro: ");
        if (!int.TryParse(Console.ReadLine(), out int codigoMembro))
        {
            throw new ArgumentException("Código do membro deve ser um número válido");
        }

        RealizarEmprestimo(idJogo, codigoMembro);
        Console.WriteLine("Jogo emprestado com sucesso!");
    }

    public void DevolverJogo()
    {
        Console.Write("ID do jogo: ");
        if (!int.TryParse(Console.ReadLine(), out int idJogo))
        {
            throw new ArgumentException("ID do jogo deve ser um número válido");
        }

        RealizarDevolucao(idJogo);
    }

    public void GerarRelatorio()
    {
        try
        {
            VerificarPasta();

            string caminhoRelatorio = Path.Combine("Data", "relatorio.txt");

            string relatorio = "\n=== RELATÓRIO DA LUDOTECA ===\n";
            relatorio += $"Data: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n";

            relatorio += "JOGOS CADASTRADOS:\n";
            for (int i = 0; i < jogos.Count; i++)
            {
                relatorio += jogos[i].ToString() + "\n";
            }

            relatorio += "\nMEMBROS CADASTRADOS:\n";
            for (int i = 0; i < membros.Count; i++)
            {
                relatorio += membros[i].ToString() + "\n";
            }

            relatorio += "\nJOGOS EMPRESTADOS:\n";
            for (int i = 0; i < jogos.Count; i++)
            {
                if (jogos[i].EstaEmprestado)
                {
                    // Buscar empréstimo mais recente
                    Emprestimo? emprestimo = null;
                    for (int j = emprestimos.Count - 1; j >= 0; j--)
                    {
                        if (emprestimos[j].IdJogo == jogos[i].Id)
                        {
                            emprestimo = emprestimos[j];
                            break;
                        }
                    }
                    
                    if (emprestimo != null)
                    {
                        string statusMulta = "";
                        if (emprestimo.EstaAtrasado())
                        {
                            statusMulta = $" | ATRASADO: {emprestimo.DiasAtraso} dias - Multa: R$ {emprestimo.ValorMulta:F2}";
                        }

                        relatorio += $"Jogo: {jogos[i].Nome} | Membro: {emprestimo.CodigoMembro} | Empréstimo: {emprestimo.DataEmprestimo:dd/MM/yyyy} | Devolução: {emprestimo.DataDevolucao:dd/MM/yyyy}{statusMulta}\n";
                    }
                }
            }

            relatorio += "\nHISTÓRICO DE EMPRÉSTIMOS:\n";
            for (int i = 0; i < emprestimos.Count; i++)
            {
                string nomeJogo = "Jogo não encontrado";
                for (int j = 0; j < jogos.Count; j++)
                {
                    if (jogos[j].Id == emprestimos[i].IdJogo)
                    {
                        nomeJogo = jogos[j].Nome;
                        break;
                    }
                }

                string infoMulta = "";
                if (emprestimos[i].MultaPaga)
                {
                    infoMulta = $" | MULTA PAGA: R$ {emprestimos[i].ValorMulta:F2} ({emprestimos[i].DiasAtraso} dias) - {emprestimos[i].MetodoPagamento}";
                }

                relatorio += $"Jogo: {nomeJogo} | Membro: {emprestimos[i].CodigoMembro} | Empréstimo: {emprestimos[i].DataEmprestimo:dd/MM/yyyy} | Devolução: {emprestimos[i].DataDevolucao:dd/MM/yyyy}{infoMulta}\n";
            }

            relatorio += "\n" + new string('-', 50) + "\n";

            File.AppendAllText(caminhoRelatorio, relatorio);
            Console.WriteLine($"Relatório anexado em: {caminhoRelatorio}");
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - GerarRelatorio: {ex.Message} | StackTrace: {ex.StackTrace}\n");
            Console.WriteLine($"Erro ao gerar relatório: {ex.Message}");
        }
    }



    public void PagarMulta(int idJogo, string formaPagamento)
    {
        decimal multa = CalcularMulta(idJogo);

        if (multa == 0)
        {
            Console.WriteLine("Não há multa para este jogo.");
            return;
        }

        string pagamento = formaPagamento.ToLower();
        if (string.Equals(pagamento, "pix", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(pagamento, "dinheiro", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Multa de R$ {multa:F2} paga via {formaPagamento}.");
        }
        else
        {
            Console.WriteLine("Forma de pagamento inválida. Use 'pix' ou 'dinheiro'.");
        }
    }

    public void ConsultarMulta()
    {
        Console.Write("ID do jogo: ");
        if (!int.TryParse(Console.ReadLine(), out int idJogo))
        {
            throw new ArgumentException("ID do jogo deve ser um número válido");
        }

        decimal multa = CalcularMulta(idJogo);
        if (multa > 0)
        {
            Console.WriteLine($"Multa: R$ {multa:F2}");
        }
        else
        {
            Console.WriteLine("Não há multa para este jogo.");
        }
    }

    public void ProcessarPagamentoMulta()
    {
        Console.Write("ID do jogo: ");
        if (!int.TryParse(Console.ReadLine(), out int idJogo))
        {
            throw new ArgumentException("ID do jogo deve ser um número válido");
        }

        Console.Write("Forma de pagamento (pix/dinheiro): ");
        string formaPagamento = Console.ReadLine() ?? "";

        PagarMulta(idJogo, formaPagamento);
    }

    public void RecarregarDados()
    {
        Console.WriteLine("\nAntes do recarregamento:");
        Console.WriteLine($"Jogos: {jogos.Count}, Membros: {membros.Count}, Empréstimos: {emprestimos.Count}");
        
        CarregarDados();
        
        Console.WriteLine("\nApós o recarregamento:");
        Console.WriteLine($"Jogos: {jogos.Count}, Membros: {membros.Count}, Empréstimos: {emprestimos.Count}");
        
        // Recalcular próximos IDs baseado nos dados carregados
        if (jogos.Count > 0)
        {
            int maiorIdJogo = 0;
            for (int i = 0; i < jogos.Count; i++)
            {
                if (jogos[i].Id > maiorIdJogo)
                    maiorIdJogo = jogos[i].Id;
            }
            proximoIdJogo = maiorIdJogo + 1;
        }
        else
        {
            proximoIdJogo = 1;
        }
        
        Console.WriteLine("\nDados recarregados do arquivo JSON!");
        Console.WriteLine($"Próximo ID de jogo: {proximoIdJogo}");
    }

}

public class DadosBiblioteca
{
    public List<Jogo> Jogos { get; set; } = new List<Jogo>();
    public List<Membro> Membros { get; set; } = new List<Membro>();
    public List<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    public int ProximoIdJogo { get; set; }
}