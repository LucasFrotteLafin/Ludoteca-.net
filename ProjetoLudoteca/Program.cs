using System.Collections;
using System.Linq.Expressions;
using LudotecaServise.Service;

class Program
{
    static void Main(string[] args)
    {
        //Cria uma instancia do servise LudotecaService, que contém os métodos que realizam as operações do sistema
        var ludoteca = new LudotecaService();

        while (true)
        {
            //Interface menu principal
            Console.Clear();
            Console.WriteLine("=== LUDOTECA .NET ===");
            Console.WriteLine("1 - Cadastrar jogo");
            Console.WriteLine("2 - Cadastrar membro");
            Console.WriteLine("3 -  Listar jogos");
            Console.WriteLine("4 - Emprestar jogo");
            Console.WriteLine("5 - Devolver jogo");
            Console.WriteLine("6 - Gerar relatório");
            Console.WriteLine("0 - Sair");
            Console.WriteLine("Opção: ");

            int opcao = Console.ReadLine();

            //switch case das opções
            try
            {
                switch (opcao)
                {
                    case "1":
                        ludoteca.CadastrarJogo();
                        break;
                    case "2":
                        ludoteca.CadastrarMembro();
                        break;
                    case "3":
                        ludoteca.ListarJogos();
                        break;
                    case "4":
                        ludoteca.EmprestarJogo();
                        break;
                    case "5":
                        ludoteca.DevolverJogo();
                        break;
                    case "6":
                        ludoteca.GerarRelatorio();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida!");
                        break;


                }
            }
            //tratamento de erro
            catch (Exception ex)
            {
                File.AppendAllText("Data/debug.log", $"[{DateTime.Now}] Erro: {ex.Message}\n");
                Console.WriteLine("Erro ao executar a operação.");
            }

            Console.WriteLine("\nPressione Enter para continuar.");
            Console.ReadLine();

        }
        
    }
}

