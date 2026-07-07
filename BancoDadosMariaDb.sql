-- SCRIPT PARA CRIAÇÃO DO BANCO DE DADOS --
CREATE DATABASE IF NOT EXISTS DBMATUTOS;
USE DBMATUTOS;

-- 1. TABELAS INDEPENDENTES (Sem chaves estrangeiras)
CREATE TABLE IF NOT EXISTS Auditoria_Log (
    Codigo_Log INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Entidade_Afetada VARCHAR(250),
    Tipo_Acao INT,
    Valor_Afetado INT,
    Usuario_Acao VARCHAR(250),
    Ativo BOOLEAN
);

CREATE TABLE IF NOT EXISTS Usuario (
    Codigo_Usuario INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(250) NOT NULL,
    E_mail VARCHAR(250) NOT NULL,
    Senha VARCHAR(250) NOT NULL,
    Codigo_Recuperacao VARCHAR(10) NULL,
    Data_Validade_Codigo DATETIME NULL,
    Ativo BOOLEAN DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Telefone (
    Codigo_Telefone INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Numero_Telefone VARCHAR(11) NOT NULL,
    Principal BIT NOT NULL,
    DDD CHAR (2)
);

CREATE TABLE IF NOT EXISTS BlackList (
    Codigo_BlackList INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Inicio_Bloqueio DATETIME,
    Fim_Bloqueio DATETIME,
    Ativo BOOLEAN DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Imagem (
    Codigo_Imagem INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Descricao VARCHAR(250),
    Imagem VARCHAR(250)
);

CREATE TABLE IF NOT EXISTS Notificacao (
    Codigo_Notificacao INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Ativo BOOLEAN DEFAULT 1,
    Descricao VARCHAR(250),
    Mensagem VARCHAR(250)
);

-- 👉 MOVIDO PARA CIMA: Precisa existir antes do Agendamento!
CREATE TABLE IF NOT EXISTS Situacao_Agendamento (
    Codigo_Situacao_Agendamento INT NOT NULL PRIMARY KEY,
    Descricao VARCHAR(250)
);

-- 2. HERANÇAS DE USUÁRIO (O ID é o mesmo do Usuário base)
CREATE TABLE IF NOT EXISTS Administrador (
    Codigo_Usuario INT NOT NULL PRIMARY KEY,
    FOREIGN KEY (Codigo_Usuario) REFERENCES Usuario (Codigo_Usuario)
);

CREATE TABLE IF NOT EXISTS Barbeiro (
    Codigo_Usuario INT NOT NULL PRIMARY KEY,
    FOREIGN KEY (Codigo_Usuario) REFERENCES Usuario (Codigo_Usuario)
);

CREATE TABLE IF NOT EXISTS Cliente (
    Codigo_Usuario INT NOT NULL PRIMARY KEY,
    FOREIGN KEY (Codigo_Usuario) REFERENCES Usuario (Codigo_Usuario)
);

-- 3. TABELAS INTERMEDIÁRIAS E DEPENDENTES DE NÍVEL 1
CREATE TABLE IF NOT EXISTS Usuario_Telefone (
    Codigo_Usuario_Telefone INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Codigo_Usuario INT NOT NULL,
    Codigo_Telefone INT NOT NULL,
    FOREIGN KEY (Codigo_Usuario) REFERENCES Usuario (Codigo_Usuario),
    FOREIGN KEY (Codigo_Telefone) REFERENCES Telefone (Codigo_Telefone)
);

CREATE TABLE IF NOT EXISTS Usuario_BlackList (
    Codigo_Usuario_BlackList INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Codigo_Usuario INT NOT NULL,
    Codigo_BlackList INT NOT NULL,
    FOREIGN KEY (Codigo_Usuario) REFERENCES Administrador (Codigo_Usuario),
    FOREIGN KEY (Codigo_BlackList) REFERENCES BlackList (Codigo_BlackList)
);

CREATE TABLE IF NOT EXISTS Servico (
    Codigo_Servico INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Descricao VARCHAR(250),
    Duracao VARCHAR(250),
    Preco DECIMAL(10,2),
    Tempo_Servico INT,
    Ativo BOOLEAN DEFAULT 1,
    Codigo_Imagem INT,
    FOREIGN KEY (Codigo_Imagem) REFERENCES Imagem (Codigo_Imagem)
);

-- 4. TABELA CENTRAL
CREATE TABLE IF NOT EXISTS Agendamento (
    Codigo_Agendamento INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Data_Agendamento DATETIME NOT NULL,
    Data_Fim_Agendamento DATETIME,
    Valor_Total_Agendamento DECIMAL(10,2), 
    Ativo BOOLEAN DEFAULT 1,
    Codigo_Cliente INT NOT NULL,
    Codigo_Barbeiro INT NOT NULL,
    Codigo_Situacao_Agendamento INT NOT NULL,
    FOREIGN KEY (Codigo_Situacao_Agendamento) REFERENCES Situacao_Agendamento (Codigo_Situacao_Agendamento),
    FOREIGN KEY (Codigo_Cliente) REFERENCES Cliente (Codigo_Usuario),
    FOREIGN KEY (Codigo_Barbeiro) REFERENCES Barbeiro (Codigo_Usuario)
);

-- 5. TABELAS DEPENDENTES DO AGENDAMENTO
CREATE TABLE IF NOT EXISTS Agendamento_Servico (
    Codigo_Agendamento_Servico INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Codigo_Agendamento INT NOT NULL,
    Codigo_Servico INT NOT NULL,
    Quantidade_Servico INT DEFAULT 1,
    Valor_Total_Item DECIMAL(10,2), 
    Tempo_Servico_Item INT,         
    Campo INT,
    FOREIGN KEY (Codigo_Agendamento) REFERENCES Agendamento (Codigo_Agendamento),
    FOREIGN KEY (Codigo_Servico) REFERENCES Servico (Codigo_Servico)
);

CREATE TABLE IF NOT EXISTS Configura_Notificacao (
    Codigo_Configura_Notificacao INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Codigo_Notificacao INT NOT NULL,
    Codigo_Agendamento INT NOT NULL,
    FOREIGN KEY (Codigo_Notificacao) REFERENCES Notificacao (Codigo_Notificacao),
    FOREIGN KEY (Codigo_Agendamento) REFERENCES Agendamento (Codigo_Agendamento)
);

-- 6. INSERTS INICIAIS DE SISTEMA
INSERT IGNORE INTO Situacao_Agendamento (Codigo_Situacao_Agendamento, Descricao) VALUES (1, 'Aberto');
INSERT IGNORE INTO Situacao_Agendamento (Codigo_Situacao_Agendamento, Descricao) VALUES (2, 'Cancelado');
INSERT IGNORE INTO Situacao_Agendamento (Codigo_Situacao_Agendamento, Descricao) VALUES (3, 'Finalizado');