using System;
using System.IO;
using System.Collections;
using System.Linq.Expressions;
using Ludoteca.Service; 

class Program
{
    static void Main(string[] args)
    {
        BibliotecaJogos ludoteca = new BibliotecaJogos()

        while (true)
        {
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

            string opcao = Console.ReadLine(); 

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

