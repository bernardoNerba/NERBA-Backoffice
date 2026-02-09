# Guia de Ciclo de Vida da Ação de Formação

Este guia descreve o processo passo-a-passo para criar, gerir e concluir uma Ação de Formação no sistema NERBA Backoffice, incluindo a gestão de todas as suas dependências.

## Visão Geral do Fluxo

O ciclo de vida de uma Ação segue a seguinte ordem lógica:

1.  **Definição do Contexto** (Enquadramento e Curso)
2.  **Criação da Ação**
3.  **Configuração Pedagógica** (Equipa e Cronograma)
4.  **Gestão de Formandos**
5.  **Acompanhamento e Conclusão**

---

## 1. Definição do Contexto (Pré-requisitos)

Antes de criar uma ação, é necessário garantir que a estrutura base existe.

### 1.1. Criar Enquadramento (Frame)
O Enquadramento define o programa de financiamento (ex: PO CH, PRR).
*   **Onde:** Menu Lateral > Enquadramentos > Botão "Criar"
*   **Dados Necessários:**
    *   Programa e Medida
    *   Código da Operação
    *   Logótipos (Programa e Financiamento) para documentos oficiais

### 1.2. Criar Curso
O Curso define a estrutura curricular que será replicada nas ações.
*   **Onde:** Menu Lateral > Cursos > Botão "Criar"
*   **Dados Necessários:**
    *   Enquadramento (selecionar o criado anteriormente)
    *   Designação do Curso
    *   Nível de Habilitação
    *   Carga Horária Total
*   **Passo Crítico:** Após criar o curso, é obrigatório **associar os Módulos** (UFCDs) que compõem o curso, definindo a carga horária de cada um. A Ação herdará estes módulos.

---

## 2. Criação da Ação

A Ação é uma instância temporal de um Curso (uma turma específica).

*   **Onde:** Menu Lateral > Ações > Botão "Criar Ação"
*   **Dados Necessários:**
    *   **Curso Base:** Selecionar o curso que vai ser ministrado.
    *   **Código Administrativo:** Código único da ação (ex: `ACAO_01_2024`).
    *   **Coordenador:** O utilizador que cria a ação fica automaticamente como coordenador (pode ser alterado).
    *   **Datas:** Data de Início e Fim previstos.
    *   **Horário/Regime:** Laboral/Pós-laboral e dias da semana habituais.
    *   **Localidade:** Local onde decorre a formação.

> **Nota:** Ao criar a ação, os módulos do curso são automaticamente associados, mas ainda não têm formadores nem datas específicas.

---

## 3. Configuração Pedagógica

Nesta fase, define-se quem dá as aulas e quando. Aceda aos **Detalhes da Ação**.

### 3.1. Associar Formadores (Equipa Pedagógica)
Para cada módulo da ação, deve indicar o formador responsável.
*   **Onde:** Detalhes da Ação > Aba "Equipa Pedagógica" (ou Módulos)
*   **Ação:** Editar um módulo ou clicar em "Associar Formador".
*   **Regras:**
    *   O formador deve estar registado no sistema.
    *   Um módulo não pode ficar sem formador se quiser agendar sessões para ele.

### 3.2. Agendar Sessões (Cronograma)
O cronograma define os dias e horas efetivos de cada aula.
*   **Onde:** Detalhes da Ação > Aba "Cronograma" ou "Sessões"
*   **Processo:**
    1.  Clicar em "Criar Sessão" (ou agendamento em massa se disponível).
    2.  Selecionar o **Módulo** e o **Formador** (preenchido automaticamente com base na associação anterior).
    3.  Definir Data, Hora Início, Hora Fim e Sala.
    4.  Escrever o Sumário (pode ser preenchido posteriormente pelo formador).

---

## 4. Gestão de Formandos

### 4.1. Inscrever Formandos
*   **Onde:** Detalhes da Ação > Aba "Formandos"
*   **Processo:**
    *   Clicar em "Adicionar Formando".
    *   Pesquisar pessoa existente ou criar nova ficha de pessoa.
    *   Definir Data de Inscrição e Estado (ex: "Em Formação").
*   **Automatismo:** Ao inscrever um formando, o sistema gera automaticamente:
    *   Registos de avaliação (vazios) para todos os módulos.
    *   Registos de assiduidade para as sessões já agendadas.

---

## 5. Acompanhamento e Conclusão

Durante a ação, o coordenador e formadores gerem o dia-a-dia.

### 5.1. Registar Assiduidade e Sumários
*   Os formadores ou coordenador devem aceder às sessões e marcar as faltas dos formandos.
*   Esta informação alimenta o processamento de pagamentos (bolsas).

### 5.2. Lançar Avaliações
*   No final de cada módulo, as notas devem ser lançadas na aba de Avaliações.

### 5.3. Gerar Documentação
A qualquer momento, pode consultar a aba **"Ficheiros"** para gerar:
*   Dossier Técnico Pedagógico.
*   Relatórios de Pagamentos.
*   Folhas de Sumário e Assiduidade.
*(Ver detalhes em [Dossier Técnico](DOSSIER_TECNICO.md))*

### 5.4. Concluir Ação
Quando a formação termina:
1.  Verificar se todas as avaliações e assiduidades estão lançadas.
2.  Aceder aos dados gerais da ação ("Editar").
3.  Alterar o **Estado** para "Concluída" ou "Terminada".
4.  Esta ação bloqueia edições futuras e arquiva a ação no histórico.
