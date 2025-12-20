using System;
using System.IO;
using Ludoteca.Services;

class Program
{
    static void Main(string[] args)
    {
        BibliotecaJogos ludoteca = new BibliotecaJogos();

        while (true)
        {
            Console.WriteLine("\n=== LUDOTECA .NET ===");
            Console.WriteLine("1 - Cadastrar jogo");
            Console.WriteLine("2 - Cadastrar membro");
            Console.WriteLine("3 - Listar jogos");
            Console.WriteLine("4 - Emprestar jogo");
            Console.WriteLine("5 - Devolver jogo");
            Console.WriteLine("6 - Gerar relatório");
            Console.WriteLine("7 - Verificar multa");
            Console.WriteLine("8 - Recarregar dados");
            Console.WriteLine("0 - Sair");
            Console.Write("Opção: ");

            string opcao = Console.ReadLine() ?? "";

            try
            {
                switch (opcao)
                {
                    case "1":
                        Console.Clear();
                        ludoteca.CadastrarJogo(); // Cadastrar novo jogo
                        break;
                    case "2":
                        Console.Clear();
                        ludoteca.CadastrarMembro(); // Cadastrar novo membro
                        break;
                    case "3":
                        Console.Clear();
                        ludoteca.ListarJogos(); // Listar todos os jogos
                        break;
                    case "4":
                        Console.Clear();
                        ludoteca.EmprestarJogo(); // Realizar empréstimo
                        break;
                    case "5":
                        Console.Clear();
                        ludoteca.DevolverJogo(); // Devolver jogo emprestado
                        break;
                    case "6":
                        Console.Clear();
                        ludoteca.GerarRelatorio(); // Gerar relatório completo
                        break;
                    case "7": 
                        Console.Clear();
                        ludoteca.VerificarMulta(); // Verificar multa por atraso
                        break;
                    case "8": 
                        Console.Clear();
                        ludoteca.RecarregarDados(); // Recarregar dados do arquivo
                        break;
                    case "0": 
                        Console.WriteLine("Saindo...");
                        return;
                    default:
                        Console.WriteLine("Opção inválida!");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Log de erro para debug
                File.AppendAllText("Data/debug.log", $"[{DateTime.Now}] Erro: {ex.Message}\n");
                Console.WriteLine("Erro ao executar a operação.");
            }

            Console.WriteLine("\nPressione Enter para continuar.");
            Console.ReadLine();
        }
    }
}