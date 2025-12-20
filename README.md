# 🎮 ProjetoLudoteca - Sistema de Gerenciamento

Sistema completo de gerenciamento de ludoteca (biblioteca de jogos) desenvolvido em C# .NET 9.0 para a disciplina de **Design e Programação Orientada a Objetos**.

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Funcionalidades](#funcionalidades)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Arquitetura](#arquitetura)
- [Como Usar](#como-usar)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Conceitos de POO](#conceitos-de-poo)
- [Autores](#autores)

## 🎯 Sobre o Projeto

O **ProjetoLudoteca** é um sistema de gerenciamento completo que simula o funcionamento de uma ludoteca real. O projeto demonstra a aplicação prática de conceitos avançados de Programação Orientada a Objetos, incluindo encapsulamento, validações robustas, persistência de dados e arquitetura em camadas.

### 🎮 O que é uma Ludoteca?
Uma ludoteca é uma biblioteca especializada em jogos, onde pessoas podem emprestar jogos de tabuleiro, cartas e outros tipos de entretenimento, similar ao sistema de empréstimo de livros em bibliotecas tradicionais.

## ⚡ Funcionalidades

### 🎲 Gestão de Jogos
- ✅ Cadastro com ID automático, nome, categoria e idade mínima
- ✅ Controle de disponibilidade (disponível/emprestado)
- ✅ Validação de idade mínima (0-18 anos)
- ✅ Listagem completa de jogos cadastrados

### 👥 Gestão de Membros
- ✅ Cadastro com código único, nome, email, telefone e data de nascimento
- ✅ Cálculo automático da idade
- ✅ Validação de email (formato padrão) e telefone (formato brasileiro)
- ✅ Verificação de idade para empréstimos

### 📅 Sistema de Empréstimos
- ✅ Empréstimos automáticos de 7 dias
- ✅ Verificação de idade (membro deve ter idade mínima do jogo)
- ✅ Controle de status ativo/devolvido
- ✅ Histórico completo de empréstimos
- ✅ Prevenção de múltiplos empréstimos por membro

### 💰 Sistema de Multas
- ✅ Cálculo automático de multas por atraso (R$ 2,50 por dia)
- ✅ Consulta de multas pendentes
- ✅ Pagamento via PIX ou dinheiro
- ✅ Registro de pagamentos realizados

### 📊 Relatórios e Persistência
- ✅ Dados salvos em JSON (Data/biblioteca.json)
- ✅ Log de erros (Data/debug.log)
- ✅ Relatórios completos (Data/relatorio.txt)
- ✅ Histórico de devoluções com timestamps
- ✅ Formato de datas brasileiro (dd/MM/yyyy)

## 🛠️ Tecnologias Utilizadas

- **C# .NET 9.0** - Framework principal
- **System.Text.Json** - Serialização e persistência de dados
- **LINQ** - Consultas otimizadas e manipulação de dados
- **Regex** - Validações de formato (email, telefone, caracteres)
- **DateTime** - Cálculos temporais precisos
- **Console Application** - Interface de usuário

## 🏗️ Arquitetura

O projeto segue uma arquitetura em camadas bem definida:

```
📁 ProjetoLudoteca/
├── 📁 Models/           # Camada de Dados
│   ├── Jogo.cs         # Entidade Jogo
│   ├── Membro.cs       # Entidade Membro
│   └── Emprestimo.cs   # Entidade Empréstimo
├── 📁 Services/         # Camada de Negócio
│   └── LudotecaService.cs # Lógica principal
├── 📁 Data/            # Persistência
│   ├── biblioteca.json # Dados do sistema
│   ├── debug.log      # Log de erros
│   └── relatorio.txt  # Relatórios gerados
└── Program.cs          # Interface do usuário
```

## 🎮 Como Usar

### Menu Principal
```
=== LUDOTECA .NET ===
1 - Cadastrar jogo
2 - Cadastrar membro
3 - Listar jogos
4 - Emprestar jogo
5 - Devolver jogo
6 - Gerar relatório
7 - Verificar multa
8 - Recarregar dados
0 - Sair
```

### Exemplos de Uso

#### 1. Cadastrar um Jogo
```
Nome do jogo: Xadrez
Categoria: Estratégia
Idade mínima: 8
```

#### 2. Cadastrar um Membro
```
Nome: João Silva
Email: joao@email.com
Telefone: 11987654321
Código do membro: 123
Data de nascimento: 15/03/1990
```

#### 3. Realizar Empréstimo
```
ID do jogo: 1
Código do membro: 123
```

## 📁 Estrutura do Projeto

### Models (Camada de Dados)
- **Encapsulamento** com propriedades `private set`
- **Validações robustas** nos construtores
- **Cálculos automáticos** (idade, multa, dias de atraso)
- **Tratamento de exceções** especializado

### Services (Camada de Negócio)
- **CRUD completo** para jogos e membros
- **Sistema de empréstimos** com regras de negócio
- **Persistência JSON** com serialização otimizada
- **Validações de entrada** com Regex
- **Otimizações LINQ** para performance

### Interface (Camada de Apresentação)
- **Menu interativo** com validação de entrada
- **Feedback claro** para o usuário
- **Tratamento de erros** com mensagens amigáveis

## 🎓 Conceitos de POO Aplicados

### ✅ Encapsulamento
```csharp
public decimal ValorMulta => Ativo ? DiasAtraso * 2.50m : 0;
public int DiasAtraso => Ativo && DateTime.Now > DataDevolucao ? 
    (DateTime.Now.Date - DataDevolucao.Date).Days : 0;
```

### ✅ Validação e Tratamento de Exceções
```csharp
private static void ValidarIdPositivo(int valor, string nomeParametro, string tipoEntidade)
{
    if (valor <= 0)
        throw new ArgumentException($"{tipoEntidade} deve ser maior que zero", nomeParametro);
}
```

### ✅ Separação de Responsabilidades
- **Models**: Entidades e regras de dados
- **Services**: Lógica de negócio e persistência
- **Program**: Interface e interação com usuário

### ✅ Otimização com LINQ
```csharp
// Busca eficiente
Jogo? jogo = jogos.FirstOrDefault(j => j.Id == idJogo);

// Verificação de duplicatas
bool jogoExiste = jogos.Any(j => string.Equals(j.Nome.Trim(), 
    nome.Trim(), StringComparison.OrdinalIgnoreCase));
```

## 🔒 Validações e Segurança

### Validações Implementadas
- ✅ **Caracteres especiais** - Regex para nomes (apenas letras)
- ✅ **Formato de email** - Validação com expressão regular
- ✅ **Telefone brasileiro** - Formato DD + 8/9 dígitos
- ✅ **Limites de tamanho** - Prevenção de overflow
- ✅ **Ranges válidos** - Idade, códigos, datas
- ✅ **Duplicatas** - Verificação case-insensitive
- ✅ **Inputs maliciosos** - Sanitização de entrada

### Tratamento de Erros
- ✅ **Try-catch** em operações críticas
- ✅ **Logs de debug** para troubleshooting
- ✅ **Mensagens amigáveis** para o usuário
- ✅ **Validação preventiva** antes de operações

## 📊 Relatórios Gerados

O sistema gera relatórios completos em `Data/relatorio.txt`:

```
=== RELATÓRIO DA LUDOTECA ===
Data: 15/12/2024 14:30

JOGOS CADASTRADOS:
[001] Xadrez | Estratégia | 8+ anos | DISPONÍVEL

MEMBROS CADASTRADOS:
João Silva | joao@email.com | 11987654321 | 34 anos

HISTÓRICO DE DEVOLUÇÕES:
Jogo: Xadrez | Membro: 123 | Devolvido em: 15/12/2024 14:25

HISTÓRICO DE EMPRÉSTIMOS:
Jogo: Xadrez | Membro: 123 | Empréstimo: 10/12/2024 | DEVOLVIDO: 15/12/2024 14:25
```

### Estrutura de Dados JSON
```json
{
  "jogos": [...],
  "membros": [...],
  "emprestimos": [...],
  "proximoIdJogo": 1,
  "proximoIdEmprestimo": 1
}
```

## 👥 Autores

- **Lucas Frotté Lafin** - *Models (implementação completa, encapsulamento, validações, exceções), Services (Persistência JSON, Empréstimos, Interface), Validações, LINQ, Tratamento de Exceções*

- **Ana Luiza Maciel Mattos** - *Models (colaboração), Program.cs (interface principal), Sistema de Multas (CalcularMulta, TestarMulta, PagarMulta), Sistema de Relatórios (GerarRelatorio), Integração e Testes*

## 🎯 Objetivos Acadêmicos Alcançados

- ✅ **Programação Orientada a Objetos** - Encapsulamento, validações, separação de responsabilidades
- ✅ **Design de Software** - Arquitetura em camadas, padrões de código
- ✅ **Tratamento de Dados** - Validações robustas, persistência JSON
- ✅ **Performance** - Otimizações LINQ, algoritmos eficientes
- ✅ **Segurança** - Validações de entrada, prevenção de ataques
- ✅ **Manutenibilidade** - Código limpo, documentação completa

---