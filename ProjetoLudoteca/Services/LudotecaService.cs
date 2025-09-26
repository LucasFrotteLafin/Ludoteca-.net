using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ludoteca.Models;

namespace Ludoteca.Services;

public class BibliotecaJogos
{
    private List<Jogo> jogos; // [AV1-2]
    private List<Membro> membros; // [AV1-2]
    private List<Emprestimo> emprestimos; // [AV1-2]
    private int proximoIdJogo; // [AV1-2]
    private int proximoIdEmprestimo; // [AV1-2]

    private readonly string caminhoArquivo; // [AV1-2]

    public BibliotecaJogos(string? caminhoPersonalizado = null)
    {
        jogos = new List<Jogo>();
        membros = new List<Membro>();
        emprestimos = new List<Emprestimo>();
        proximoIdJogo = 1;
        proximoIdEmprestimo = 1;
        caminhoArquivo = caminhoPersonalizado ?? "data/biblioteca.json";

        try
        {
            VerificarPasta();
            CarregarDados();
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now}] Erro na inicialização: {ex.Message}\n");
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
        // Assertiva de consistência
        if (jogos == null)
            throw new InvalidOperationException("Lista de jogos não inicializada");

        // Verificar se jogo já existe
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
        proximoIdJogo++;
        SalvarDados();
    }

    public void AdicionarMembro(string nome, string email, string telefone, int codigoMembro, DateTime dataNascimento)
    {
        // Assertiva de consistência
        if (membros == null)
            throw new InvalidOperationException("Lista de membros não inicializada");

        // Verificar se email já existe
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

        // Verificar se código já existe
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
        // Assertivas de consistência
        if (jogos == null || membros == null || emprestimos == null)
            throw new InvalidOperationException("Listas não inicializadas");

        // Buscar jogo
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

        // Buscar membro
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
            Console.WriteLine($"Membros cadastrados: {string.Join(", ", membros.Select(m => m.CodigoMembro))}");
            throw new ArgumentException($"Código do membro {codigoMembro} não encontrado!");
        }

        // Verificar idade para o jogo
        if (!membro.PodeAlugarJogo(jogo.IdadeMinima))
        {
            throw new InvalidOperationException($"Membro tem {membro.Idade} anos. Idade mínima para este jogo: {jogo.IdadeMinima} anos");
        }

        if (jogo.EstaEmprestado)
        {
            throw new InvalidOperationException("Jogo já está emprestado!");
        }

        jogo.MarcarComoEmprestado();
        Emprestimo novoEmprestimo = new Emprestimo(proximoIdEmprestimo, idJogo, codigoMembro, 7);
        emprestimos.Add(novoEmprestimo);
        proximoIdEmprestimo++;
        SalvarDados();
    }

    public void RealizarDevolucao(int idJogo)
    {
        // Assertivas de consistência
        if (jogos == null || emprestimos == null)
            throw new InvalidOperationException("Listas não inicializadas");

        // Buscar jogo
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

        // Buscar empréstimo ativo
        Emprestimo? emprestimo = null;
        for (int i = 0; i < emprestimos.Count; i++)
        {
            if (emprestimos[i].IdJogo == idJogo && emprestimos[i].Ativo)
            {
                emprestimo = emprestimos[i];
                break;
            }
        }

        if (emprestimo == null)
        {
            throw new InvalidOperationException("Jogo não está emprestado!");
        }

        if (emprestimo.EstaAtrasado())
        {
            decimal multa = CalcularMulta(idJogo);
            Console.WriteLine($"Jogo está atrasado! Multa: R$ {multa:F2}");
            Console.Write("Deseja pagar a multa agora? (s/n): ");
            string resposta = Console.ReadLine()?.ToLower() ?? "";
            
            if (resposta == "s" || resposta == "sim")
            {
                Console.Write("Forma de pagamento (pix/dinheiro): ");
                string formaPagamento = Console.ReadLine() ?? "";
                
                if (formaPagamento.ToLower() == "pix" || formaPagamento.ToLower() == "dinheiro")
                {
                    // Calcular dias de atraso
            int diasAtr-aso = 0;
            for (int i = 0; i < emprestimos.Count; i++)
            {
                if (emprestimos[i].IdJogo == idJogo && emprestimos[i].Ativo)
                {
                    diasAtraso = (DateTime.Now - emprestimos[i].DataDevolucao).Days;
                    break;
                }
            }
            Console.WriteLine($"Multa de R$ {multa:F2} ({diasAtraso} dias excedidos) paga via {formaPagamento}.");
                }
                else
                {
                    Console.WriteLine("Forma de pagamento inválida. Multa não paga.");
                }
            }
            else
            {
                Console.WriteLine("Multa não paga. Você pode pagar depois pelo menu principal.");
            }
        }

        emprestimo.Devolver();
        jogo.MarcarComoDevolvido();
        SalvarDados();
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
            dados.ProximoIdEmprestimo = proximoIdEmprestimo;

            JsonSerializerOptions opcoes = new JsonSerializerOptions();
            opcoes.WriteIndented = true;
            opcoes.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            string json = JsonSerializer.Serialize(dados, opcoes); // [AV1-3]
            File.WriteAllText(caminhoArquivo, json);
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now}] Erro ao salvar dados: {ex.Message}\n");
            Console.WriteLine($"Erro ao salvar dados: {ex.Message}");
        }
    }

    public void CarregarDados()
    {
        try
        {
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

            DadosBiblioteca? dados = JsonSerializer.Deserialize<DadosBiblioteca>(json, opcoes); // [AV1-3]

            if (dados != null)
            {
                if (dados.Jogos != null)
                    jogos = dados.Jogos;

                if (dados.Membros != null)
                    membros = dados.Membros;

                if (dados.Emprestimos != null)
                    emprestimos = dados.Emprestimos;

                proximoIdJogo = dados.ProximoIdJogo;
                proximoIdEmprestimo = dados.ProximoIdEmprestimo;
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now}] Erro ao carregar dados: {ex.Message}\n");
            Console.WriteLine($"Erro ao carregar dados: {ex.Message}");
        }
    }

    public void CadastrarJogo()
    {
        Console.Write("Nome do jogo: ");
        string nome = Console.ReadLine() ?? "";
        Console.Write("Categoria: ");
        string categoria = Console.ReadLine() ?? "";
        Console.Write("Idade mínima: ");
        
        if (!int.TryParse(Console.ReadLine(), out int idade))
        {
            throw new ArgumentException("Idade deve ser um número válido");
        }
        
        AdicionarJogo(nome, categoria, idade);
        Console.WriteLine("Jogo cadastrado com sucesso!");
    }

    public void CadastrarMembro()
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
            throw new ArgumentException("Código deve ser um número válido");
        }
        
        Console.Write("Data de nascimento (dd/MM/yyyy): ");
        if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataNascimento))
        {
            throw new ArgumentException("Data deve estar no formato dd/MM/yyyy");
        }
        
        AdicionarMembro(nome, email, telefone, codigoMembro, dataNascimento);
        Console.WriteLine("Membro cadastrado com sucesso!");
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
        Console.WriteLine("Jogo devolvido com sucesso!");
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

            relatorio += "\nEMPRÉSTIMOS ATIVOS:\n";
            for (int i = 0; i < emprestimos.Count; i++)
            {
                if (emprestimos[i].Ativo)
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

                    relatorio += $"ID: {emprestimos[i].Id} | Jogo: {nomeJogo} | Membro: {emprestimos[i].CodigoMembro} | Empréstimo: {emprestimos[i].DataEmprestimo:dd/MM/yyyy} | Devolução: {emprestimos[i].DataDevolucao:dd/MM/yyyy} | Status: Ativo\n";
                }
            }

            relatorio += "\nHISTÓRICO DE EMPRÉSTIMOS DEVOLVIDOS:\n";
            for (int i = 0; i < emprestimos.Count; i++)
            {
                if (!emprestimos[i].Ativo)
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

                    relatorio += $"ID: {emprestimos[i].Id} | Jogo: {nomeJogo} | Membro: {emprestimos[i].CodigoMembro} | Alugado: {emprestimos[i].DataEmprestimo:dd/MM/yyyy} | Devolvido: {DateTime.Now:dd/MM/yyyy}\n";
                }
            }

            relatorio += "\n" + new string('-', 50) + "\n";

            File.AppendAllText(caminhoRelatorio, relatorio);
            Console.WriteLine($"Relatório anexado em: {caminhoRelatorio}");
        }
        catch (Exception ex)
        {
            File.AppendAllText("Data/debug.log", $"[{DateTime.Now}] Erro ao gerar relatório: {ex.Message}\n");
            Console.WriteLine($"Erro ao gerar relatório: {ex.Message}");
        }
    }

    public decimal CalcularMulta(int idJogo)
    {
        for (int i = 0; i < emprestimos.Count; i++)
        {
            if (emprestimos[i].IdJogo == idJogo && emprestimos[i].Ativo)
            {
                DateTime hoje = DateTime.Now;
                int diasAtraso = (hoje.Date - emprestimos[i].DataDevolucao.Date).Days;
                if (diasAtraso > 0)
                    return diasAtraso * 2.50m;
            }
        }
        return 0;
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
            Console.WriteLine($"Multa de R$ {multa:F2}  paga via {formaPagamento}.");
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

    public void VerificarMulta()
    {
        int idJogo;
        while (true)
        {
            Console.Write("ID do jogo para verificar multa: ");
            if (int.TryParse(Console.ReadLine(), out idJogo))
                break;
            Console.WriteLine("Por favor, digite um número válido.");
        }

        decimal multa = CalcularMulta(idJogo);
        Console.WriteLine($"Multa calculada: R$ {multa:F2}");
    }

    public void RecarregarDados()
    {
        CarregarDados();
        Console.WriteLine("Dados recarregados do arquivo JSON!");
    }

}

public class DadosBiblioteca
{
    public List<Jogo> Jogos { get; set; } = new List<Jogo>();
    public List<Membro> Membros { get; set; } = new List<Membro>();
    public List<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    public int ProximoIdJogo { get; set; }
    public int ProximoIdEmprestimo { get; set; }
}