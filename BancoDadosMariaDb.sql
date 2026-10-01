-- --------------------------------------------------------
-- Servidor:                     127.0.0.1
-- Versão do servidor:           12.2.2-MariaDB - MariaDB Server
-- OS do Servidor:               Win64
-- HeidiSQL Versão:              12.17.0.7270
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

CREATE DATABASE IF NOT EXISTS `dbmatutos` /*!40100 DEFAULT CHARACTER SET utf8mb4 */;
USE `dbmatutos`;

-- Copiando estrutura para tabela dbmatutos.administrador
CREATE TABLE IF NOT EXISTS `administrador` (
  `Codigo_Usuario` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.administrador: ~0 rows (aproximadamente)
INSERT INTO `administrador` (`Codigo_Usuario`) VALUES
	(2);

-- Copiando estrutura para tabela dbmatutos.agendamento
CREATE TABLE IF NOT EXISTS `agendamento` (
  `Codigo_Agendamento` int(11) NOT NULL AUTO_INCREMENT,
  `Data_Agendamento` datetime NOT NULL,
  `Data_Fim_Agendamento` datetime DEFAULT NULL,
  `Codigo_Cliente` int(11) NOT NULL,
  `Codigo_Barbeiro` int(11) NOT NULL,
  `Codigo_Situacao_Agendamento` int(11) NOT NULL,
  `Valor_Total_Agendamento` decimal(18,2) DEFAULT NULL,
  `Ativo` tinyint(1) NOT NULL,
  PRIMARY KEY (`Codigo_Agendamento`),
  KEY `Codigo_Cliente` (`Codigo_Cliente`),
  KEY `Codigo_Barbeiro` (`Codigo_Barbeiro`),
  KEY `3` (`Codigo_Situacao_Agendamento`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Cliente`) REFERENCES `cliente` (`Codigo_Usuario`),
  CONSTRAINT `2` FOREIGN KEY (`Codigo_Barbeiro`) REFERENCES `barbeiro` (`Codigo_Usuario`),
  CONSTRAINT `3` FOREIGN KEY (`Codigo_Situacao_Agendamento`) REFERENCES `situacao_agendamento` (`Codigo_Situacao_Agendamento`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.agendamento: ~17 rows (aproximadamente)
INSERT INTO `agendamento` (`Codigo_Agendamento`, `Data_Agendamento`, `Data_Fim_Agendamento`, `Codigo_Cliente`, `Codigo_Barbeiro`, `Codigo_Situacao_Agendamento`, `Valor_Total_Agendamento`, `Ativo`) VALUES
	(2, '2026-09-15 09:00:00', '2026-09-15 10:00:00', 1, 9, 4, 75.00, 1),
	(3, '2026-09-16 09:00:00', NULL, 1, 9, 2, 0.00, 0),
	(4, '2026-09-16 09:00:00', NULL, 1, 9, 2, 0.00, 0),
	(5, '2026-09-16 09:00:00', '2026-09-16 10:00:00', 1, 9, 4, 75.00, 1),
	(6, '2026-09-16 10:00:00', NULL, 1, 9, 2, 0.00, 0),
	(7, '2026-09-17 10:00:00', NULL, 1, 9, 2, 0.00, 0),
	(8, '2026-09-16 10:00:00', '2026-09-16 10:20:00', 1, 9, 2, 25.00, 1),
	(9, '2026-09-18 10:00:00', '2026-09-18 11:50:00', 1, 9, 3, 115.00, 1),
	(10, '2026-09-18 09:00:00', NULL, 1, 9, 2, 0.00, 0),
	(11, '2026-09-25 10:00:00', NULL, 1, 9, 2, 0.00, 0),
	(12, '2026-09-25 10:00:00', NULL, 1, 9, 2, 0.00, 0),
	(13, '2026-09-25 10:00:00', '2026-09-25 11:05:00', 1, 9, 2, 70.00, 1),
	(14, '2026-09-25 11:00:00', '2026-09-25 12:00:00', 1, 9, 4, 75.00, 1),
	(15, '2026-09-28 09:00:00', '2026-09-28 09:40:00', 1, 12, 2, 50.00, 1),
	(16, '2026-09-28 09:00:00', '2026-09-28 10:25:00', 1, 11, 3, 95.00, 1),
	(17, '2026-09-28 07:30:00', '2026-09-28 08:10:00', 1, 11, 3, 50.00, 1),
	(18, '2026-09-28 08:30:00', '2026-09-28 08:50:00', 1, 11, 3, 25.00, 1);

-- Copiando estrutura para tabela dbmatutos.agendamento_servico
CREATE TABLE IF NOT EXISTS `agendamento_servico` (
  `Codigo_Agendamento_Servico` int(11) NOT NULL AUTO_INCREMENT,
  `Codigo_Agendamento` int(11) NOT NULL,
  `Codigo_Servico` int(11) NOT NULL,
  `Quantidade_Servico` int(11) DEFAULT 1,
  `Campo` int(11) DEFAULT NULL,
  `Valor_Total_Item` decimal(18,2) DEFAULT NULL,
  `Tempo_Servico_Item` int(11) DEFAULT NULL,
  PRIMARY KEY (`Codigo_Agendamento_Servico`),
  KEY `Codigo_Agendamento` (`Codigo_Agendamento`),
  KEY `Codigo_Servico` (`Codigo_Servico`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Agendamento`) REFERENCES `agendamento` (`Codigo_Agendamento`),
  CONSTRAINT `2` FOREIGN KEY (`Codigo_Servico`) REFERENCES `servico` (`Codigo_Servico`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.agendamento_servico: ~13 rows (aproximadamente)
INSERT INTO `agendamento_servico` (`Codigo_Agendamento_Servico`, `Codigo_Agendamento`, `Codigo_Servico`, `Quantidade_Servico`, `Campo`, `Valor_Total_Item`, `Tempo_Servico_Item`) VALUES
	(1, 2, 1, 3, NULL, 75.00, 60),
	(2, 5, 1, 3, NULL, 75.00, 60),
	(3, 8, 1, 1, NULL, 25.00, 20),
	(4, 9, 1, 1, NULL, 25.00, 20),
	(5, 9, 2, 2, NULL, 90.00, 90),
	(6, 13, 1, 1, NULL, 25.00, 20),
	(7, 13, 2, 1, NULL, 45.00, 45),
	(8, 14, 1, 3, NULL, 75.00, 60),
	(9, 15, 1, 2, NULL, 50.00, 40),
	(10, 16, 1, 2, NULL, 50.00, 40),
	(11, 16, 2, 1, NULL, 45.00, 45),
	(12, 17, 1, 2, NULL, 50.00, 40),
	(13, 18, 1, 1, NULL, 25.00, 20);

-- Copiando estrutura para tabela dbmatutos.auditoria_log
CREATE TABLE IF NOT EXISTS `auditoria_log` (
  `Codigo_Log` bigint(20) NOT NULL AUTO_INCREMENT,
  `Entidade_Afetada` varchar(250) NOT NULL,
  `Registro_ID` varchar(255) NOT NULL,
  `Tipo_Acao` varchar(50) NOT NULL,
  `Valores_Antigos` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`Valores_Antigos`)),
  `Valores_Novos` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`Valores_Novos`)),
  `Usuario_Acao` int(11) DEFAULT NULL,
  `Data_Hora_Acao` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`Codigo_Log`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.auditoria_log: ~1 rows (aproximadamente)
INSERT INTO `auditoria_log` (`Codigo_Log`, `Entidade_Afetada`, `Registro_ID`, `Tipo_Acao`, `Valores_Antigos`, `Valores_Novos`, `Usuario_Acao`, `Data_Hora_Acao`) VALUES
	(1, 'Servico', '-2147482647', 'INSERT', NULL, '{"Codigo_Servico":-2147482647,"Ativo":true,"Descricao":"teste","Duracao":"10","Preco":10,"Tempo_Servico":10}', 2, '2026-10-01 00:53:45');

-- Copiando estrutura para tabela dbmatutos.barbeiro
CREATE TABLE IF NOT EXISTS `barbeiro` (
  `Codigo_Usuario` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.barbeiro: ~1 rows (aproximadamente)
INSERT INTO `barbeiro` (`Codigo_Usuario`) VALUES
	(9),
	(11),
	(12);

-- Copiando estrutura para tabela dbmatutos.blacklist
CREATE TABLE IF NOT EXISTS `blacklist` (
  `Codigo_Blacklist` int(11) NOT NULL AUTO_INCREMENT,
  `Inicio_Bloqueio` datetime NOT NULL,
  `Fim_Bloqueio` datetime NOT NULL,
  `Ativo` tinyint(1) DEFAULT 1,
  `Codigo_Agendamento` int(11) DEFAULT NULL,
  `Detalhes` varchar(250) DEFAULT NULL,
  PRIMARY KEY (`Codigo_Blacklist`) USING BTREE,
  KEY `FK_blacklist_agendamento` (`Codigo_Agendamento`),
  CONSTRAINT `FK_blacklist_agendamento` FOREIGN KEY (`Codigo_Agendamento`) REFERENCES `agendamento` (`Codigo_Agendamento`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.blacklist: ~15 rows (aproximadamente)
INSERT INTO `blacklist` (`Codigo_Blacklist`, `Inicio_Bloqueio`, `Fim_Bloqueio`, `Ativo`, `Codigo_Agendamento`, `Detalhes`) VALUES
	(1, '2026-09-12 00:00:00', '2026-09-12 00:00:00', 1, NULL, 'Bloqueio sem motivo'),
	(2, '2026-09-10 00:00:00', '2026-09-14 13:30:00', 1, NULL, 'Bloqueio sem motivo'),
	(3, '2026-09-15 09:00:00', '2026-09-15 10:00:00', 1, 2, 'Horário bloqueado oriundo do agendamento 2.'),
	(4, '2026-09-16 09:00:00', '2026-09-16 10:00:00', 1, 5, 'Horário bloqueado oriundo do agendamento 5.'),
	(5, '2026-09-16 10:00:00', '2026-09-16 10:20:00', 0, 8, 'Horário bloqueado oriundo do agendamento 8.'),
	(6, '2026-09-18 10:00:00', '2026-09-18 11:50:00', 1, 9, 'Horário bloqueado oriundo do agendamento 9.'),
	(7, '2026-09-21 00:00:00', '2026-09-21 13:00:00', 1, NULL, 'UELER TESTE'),
	(8, '2026-09-21 00:00:00', '2026-09-21 00:00:00', 1, NULL, 'TESTE'),
	(9, '2026-09-25 10:00:00', '2026-09-25 11:05:00', 0, 13, 'Horário bloqueado oriundo do agendamento 13.'),
	(10, '2026-09-25 11:00:00', '2026-09-25 12:00:00', 1, 14, 'Horário bloqueado oriundo do agendamento 14.'),
	(11, '2026-09-28 09:00:00', '2026-09-28 09:40:00', 0, 15, 'Horário bloqueado oriundo do agendamento 15.'),
	(12, '2026-09-28 09:00:00', '2026-09-28 10:25:00', 1, 16, 'Horário bloqueado oriundo do agendamento 16.'),
	(13, '2026-09-28 12:00:00', '2026-09-28 13:00:00', 1, NULL, 'TESTE UELER'),
	(14, '2026-09-28 07:30:00', '2026-09-28 08:10:00', 1, 17, 'Horário bloqueado oriundo do agendamento 17.'),
	(15, '2026-09-28 08:30:00', '2026-09-28 08:50:00', 1, 18, 'Horário bloqueado oriundo do agendamento 18.');

-- Copiando estrutura para tabela dbmatutos.cliente
CREATE TABLE IF NOT EXISTS `cliente` (
  `Codigo_Usuario` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.cliente: ~7 rows (aproximadamente)
INSERT INTO `cliente` (`Codigo_Usuario`) VALUES
	(1),
	(3),
	(4),
	(5),
	(6),
	(7),
	(8),
	(10);

-- Copiando estrutura para tabela dbmatutos.configura_notificacao
CREATE TABLE IF NOT EXISTS `configura_notificacao` (
  `Codigo_Notificacao` int(11) NOT NULL AUTO_INCREMENT,
  `Ativo` tinyint(1) DEFAULT 1,
  `Codigo_Tipo` int(11) NOT NULL COMMENT 'Chave Estrangeira ligada à tabela Tipo_Evento',
  `Descricao` varchar(250) DEFAULT NULL,
  `Mensagem` varchar(250) DEFAULT NULL,
  `Valor` int(11) DEFAULT NULL,
  `UnidadeTempo` int(11) DEFAULT NULL COMMENT '1 = Minutos, 2 = Horas, 3 = Dias',
  `DataCadastro` datetime DEFAULT NULL,
  PRIMARY KEY (`Codigo_Notificacao`),
  KEY `FK_Configura_TipoEvento` (`Codigo_Tipo`),
  CONSTRAINT `FK_Configura_TipoEvento` FOREIGN KEY (`Codigo_Tipo`) REFERENCES `tipo_evento` (`Codigo_Tipo`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Copiando dados para a tabela dbmatutos.configura_notificacao: ~12 rows (aproximadamente)
INSERT INTO `configura_notificacao` (`Codigo_Notificacao`, `Ativo`, `Codigo_Tipo`, `Descricao`, `Mensagem`, `Valor`, `UnidadeTempo`, `DataCadastro`) VALUES
	(1, 1, 3, 'PROMOÇÃO DE 10%  NO MÊS DE AGOSTO', 'Caro cliente, no mês de Agosto todos os serviços terão 10% de desconto, não perca tempo marque seu horário', NULL, NULL, '2026-09-21 20:50:52'),
	(2, 0, 3, 'promoção Teste', 'TESTE PROMOÇÃO', NULL, NULL, '2026-09-21 20:50:54'),
	(3, 1, 3, 'Promoção de DDDZ', 'No mês de setembro todos os serviços terão 20% de desconto', NULL, NULL, '2026-09-21 20:50:55'),
	(4, 0, 3, 'Promoção do mês de Outubro', 'Crianças de  5 anos à 8 não pagam pelo serviço!!', NULL, NULL, '2026-09-21 20:50:57'),
	(5, 1, 3, 'PROMOÇÃO TESTE', 'TESTE PROMOÇÃO ', NULL, NULL, NULL),
	(6, 1, 3, 'TESTE PROMOÇÃO DEZEMBRO', 'PROMOÇÃO DE DEZEMBRO PAPAI VENHA APROVEITAR E SENTAR NO COLO DO NOEL!', NULL, NULL, NULL),
	(7, 1, 3, 'promoção de setembro', 'Promoção de 15% em setembro venha cortar seu cabelo!!', NULL, NULL, NULL),
	(8, 1, 3, 'PROMOÇÃO DE SETEMBRO PAPAI', 'PROMOÇÃO VENHA CORTAR LUIZADAS!!!!', NULL, NULL, NULL),
	(9, 1, 3, 'Teste Ueler', 'Teste Uéler', NULL, NULL, NULL),
	(10, 1, 3, 'teste Uéler Bernardo', 'teste ueler bernardo tessste', NULL, NULL, NULL),
	(11, 1, 3, 'teste ueler', 'ueler teste', NULL, NULL, NULL),
	(12, 1, 3, 'anuncio teste promocional', 'anuncio teste promocional', NULL, NULL, '2026-09-21 20:50:59');

-- Copiando estrutura para tabela dbmatutos.notificacao
CREATE TABLE IF NOT EXISTS `notificacao` (
  `Codigo_Historico` int(11) NOT NULL AUTO_INCREMENT,
  `Codigo_Notificacao` int(11) NOT NULL COMMENT 'Qual regra gerou este disparo',
  `Codigo_Usuario` int(11) NOT NULL COMMENT 'Quem vai receber o aviso',
  `Codigo_Agendamento` int(11) DEFAULT NULL COMMENT 'Pode ser nulo se for notificação de inatividade',
  `MensagemEnviada` varchar(500) NOT NULL COMMENT 'O texto final processado com o nome/hora',
  `DataDisparo` datetime NOT NULL,
  `Lida` tinyint(1) DEFAULT 0 COMMENT '0 = Não lida, 1 = Lida pelo app',
  PRIMARY KEY (`Codigo_Historico`),
  KEY `FK_Notificacao_Configura` (`Codigo_Notificacao`),
  KEY `FK_Notificacao_Usuario` (`Codigo_Usuario`),
  KEY `FK_Notificacao_Agendamento` (`Codigo_Agendamento`),
  CONSTRAINT `FK_Notificacao_Agendamento` FOREIGN KEY (`Codigo_Agendamento`) REFERENCES `agendamento` (`Codigo_Agendamento`),
  CONSTRAINT `FK_Notificacao_Configura` FOREIGN KEY (`Codigo_Notificacao`) REFERENCES `configura_notificacao` (`Codigo_Notificacao`),
  CONSTRAINT `FK_Notificacao_Usuario` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`)
) ENGINE=InnoDB AUTO_INCREMENT=70 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Copiando dados para a tabela dbmatutos.notificacao: ~69 rows (aproximadamente)
INSERT INTO `notificacao` (`Codigo_Historico`, `Codigo_Notificacao`, `Codigo_Usuario`, `Codigo_Agendamento`, `MensagemEnviada`, `DataDisparo`, `Lida`) VALUES
	(1, 2, 1, NULL, 'Olá, cliente! Caro cliente, no mês de Agosto todos os serviços terão 10% de desconto, não perca tempo marque seu horário', '2026-08-18 21:08:10', 1),
	(2, 2, 1, NULL, 'Olá, cliente! TESTE PROMOÇÃO', '2026-08-18 21:11:58', 1),
	(3, 2, 1, NULL, 'Olá, cliente! Caro cliente, no mês de Agosto todos os serviços terão 10% de desconto, não perca tempo marque seu horário', '2026-08-18 21:12:58', 1),
	(4, 2, 1, NULL, 'Olá, cliente! Caro cliente, no mês de Agosto todos os serviços terão 10% de desconto, não perca tempo marque seu horário', '2026-08-18 21:26:57', 1),
	(5, 1, 1, NULL, 'Olá, cliente! Caro cliente, no mês de Agosto todos os serviços terão 10% de desconto, não perca tempo marque seu horário', '2026-08-18 21:28:57', 1),
	(6, 3, 1, NULL, 'Olá, cliente! No mês de setembro todos os serviços terão 20% de desconto', '2026-09-12 16:01:11', 1),
	(7, 3, 4, NULL, 'Olá, Teste! No mês de setembro todos os serviços terão 20% de desconto', '2026-09-12 16:01:11', 0),
	(8, 3, 5, NULL, 'Olá, luiz! No mês de setembro todos os serviços terão 20% de desconto', '2026-09-12 16:01:11', 0),
	(9, 3, 6, NULL, 'Olá, joao! No mês de setembro todos os serviços terão 20% de desconto', '2026-09-12 16:01:11', 0),
	(10, 3, 7, NULL, 'Olá, TESTE! No mês de setembro todos os serviços terão 20% de desconto', '2026-09-12 16:01:11', 0),
	(11, 3, 8, NULL, 'Olá, Uriel! No mês de setembro todos os serviços terão 20% de desconto', '2026-09-12 16:01:11', 0),
	(12, 3, 2, NULL, 'Olá, cliente! Crianças de  5 anos à 8 não pagam pelo serviço!!', '2026-09-12 16:04:11', 1),
	(13, 4, 4, NULL, 'Olá, Teste! Crianças de  5 anos à 8 não pagam pelo serviço!!', '2026-09-12 16:04:11', 0),
	(14, 4, 5, NULL, 'Olá, luiz! Crianças de  5 anos à 8 não pagam pelo serviço!!', '2026-09-12 16:04:11', 0),
	(15, 4, 6, NULL, 'Olá, joao! Crianças de  5 anos à 8 não pagam pelo serviço!!', '2026-09-12 16:04:11', 0),
	(16, 4, 7, NULL, 'Olá, TESTE! Crianças de  5 anos à 8 não pagam pelo serviço!!', '2026-09-12 16:04:11', 0),
	(17, 4, 8, NULL, 'Olá, Uriel! Crianças de  5 anos à 8 não pagam pelo serviço!!', '2026-09-12 16:04:11', 0),
	(18, 5, 1, NULL, 'Olá, cliente! TESTE PROMOÇÃO ', '2026-09-12 16:11:11', 1),
	(19, 5, 4, NULL, 'Olá, Teste! TESTE PROMOÇÃO ', '2026-09-12 16:11:11', 0),
	(20, 5, 5, NULL, 'Olá, luiz! TESTE PROMOÇÃO ', '2026-09-12 16:11:11', 0),
	(21, 5, 6, NULL, 'Olá, joao! TESTE PROMOÇÃO ', '2026-09-12 16:11:11', 0),
	(22, 5, 7, NULL, 'Olá, TESTE! TESTE PROMOÇÃO ', '2026-09-12 16:11:11', 0),
	(23, 5, 8, NULL, 'Olá, Uriel! TESTE PROMOÇÃO ', '2026-09-12 16:11:11', 0),
	(24, 6, 1, NULL, 'Olá, cliente! PROMOÇÃO DE DEZEMBRO PAPAI VENHA APROVEITAR E SENTAR NO COLO DO NOEL!', '2026-09-12 16:27:11', 1),
	(25, 6, 4, NULL, 'Olá, Teste! PROMOÇÃO DE DEZEMBRO PAPAI VENHA APROVEITAR E SENTAR NO COLO DO NOEL!', '2026-09-12 16:27:11', 0),
	(26, 6, 5, NULL, 'Olá, luiz! PROMOÇÃO DE DEZEMBRO PAPAI VENHA APROVEITAR E SENTAR NO COLO DO NOEL!', '2026-09-12 16:27:11', 0),
	(27, 6, 6, NULL, 'Olá, joao! PROMOÇÃO DE DEZEMBRO PAPAI VENHA APROVEITAR E SENTAR NO COLO DO NOEL!', '2026-09-12 16:27:11', 0),
	(28, 6, 7, NULL, 'Olá, TESTE! PROMOÇÃO DE DEZEMBRO PAPAI VENHA APROVEITAR E SENTAR NO COLO DO NOEL!', '2026-09-12 16:27:11', 0),
	(29, 6, 8, NULL, 'Olá, Uriel! PROMOÇÃO DE DEZEMBRO PAPAI VENHA APROVEITAR E SENTAR NO COLO DO NOEL!', '2026-09-12 16:27:11', 0),
	(30, 7, 1, NULL, 'Olá, cliente! Promoção de 15% em setembro venha cortar seu cabelo!!', '2026-09-15 20:54:26', 1),
	(31, 7, 4, NULL, 'Olá, Teste! Promoção de 15% em setembro venha cortar seu cabelo!!', '2026-09-15 20:54:34', 0),
	(32, 7, 5, NULL, 'Olá, luiz! Promoção de 15% em setembro venha cortar seu cabelo!!', '2026-09-15 20:54:34', 0),
	(33, 7, 6, NULL, 'Olá, joao! Promoção de 15% em setembro venha cortar seu cabelo!!', '2026-09-15 20:54:34', 0),
	(34, 7, 7, NULL, 'Olá, TESTE! Promoção de 15% em setembro venha cortar seu cabelo!!', '2026-09-15 20:54:34', 0),
	(35, 7, 8, NULL, 'Olá, Uriel! Promoção de 15% em setembro venha cortar seu cabelo!!', '2026-09-15 20:54:34', 0),
	(36, 8, 1, NULL, 'Olá, cliente! PROMOÇÃO VENHA CORTAR LUIZADAS!!!!', '2026-09-15 20:55:26', 1),
	(37, 8, 4, NULL, 'Olá, Teste! PROMOÇÃO VENHA CORTAR LUIZADAS!!!!', '2026-09-15 20:55:30', 0),
	(38, 8, 5, NULL, 'Olá, luiz! PROMOÇÃO VENHA CORTAR LUIZADAS!!!!', '2026-09-15 20:55:30', 0),
	(39, 8, 6, NULL, 'Olá, joao! PROMOÇÃO VENHA CORTAR LUIZADAS!!!!', '2026-09-15 20:55:30', 0),
	(40, 8, 7, NULL, 'Olá, TESTE! PROMOÇÃO VENHA CORTAR LUIZADAS!!!!', '2026-09-15 20:55:30', 0),
	(41, 8, 8, NULL, 'Olá, Uriel! PROMOÇÃO VENHA CORTAR LUIZADAS!!!!', '2026-09-15 20:55:30', 0),
	(42, 9, 1, NULL, 'Olá, cliente! Teste Uéler', '2026-09-15 21:34:30', 1),
	(43, 9, 4, NULL, 'Olá, Teste! Teste Uéler', '2026-09-15 21:34:32', 0),
	(44, 9, 5, NULL, 'Olá, luiz! Teste Uéler', '2026-09-15 21:34:32', 0),
	(45, 9, 6, NULL, 'Olá, joao! Teste Uéler', '2026-09-15 21:34:32', 0),
	(46, 9, 7, NULL, 'Olá, TESTE! Teste Uéler', '2026-09-15 21:34:32', 0),
	(47, 9, 8, NULL, 'Olá, Uriel! Teste Uéler', '2026-09-15 21:34:32', 0),
	(48, 9, 10, NULL, 'Olá, Luiz! Teste Uéler', '2026-09-15 21:34:32', 0),
	(49, 10, 1, NULL, 'Olá, cliente! teste ueler bernardo tessste', '2026-09-15 21:38:30', 1),
	(50, 10, 4, NULL, 'Olá, Teste! teste ueler bernardo tessste', '2026-09-15 21:38:31', 0),
	(51, 10, 5, NULL, 'Olá, luiz! teste ueler bernardo tessste', '2026-09-15 21:38:31', 0),
	(52, 10, 6, NULL, 'Olá, joao! teste ueler bernardo tessste', '2026-09-15 21:38:31', 0),
	(53, 10, 7, NULL, 'Olá, TESTE! teste ueler bernardo tessste', '2026-09-15 21:38:31', 0),
	(54, 10, 8, NULL, 'Olá, Uriel! teste ueler bernardo tessste', '2026-09-15 21:38:31', 0),
	(55, 10, 10, NULL, 'Olá, Luiz! teste ueler bernardo tessste', '2026-09-15 21:38:31', 0),
	(56, 11, 1, NULL, 'Olá, cliente! ueler teste', '2026-09-15 21:39:30', 1),
	(57, 11, 4, NULL, 'Olá, Teste! ueler teste', '2026-09-15 21:39:35', 0),
	(58, 11, 5, NULL, 'Olá, luiz! ueler teste', '2026-09-15 21:39:35', 0),
	(59, 11, 6, NULL, 'Olá, joao! ueler teste', '2026-09-15 21:39:35', 0),
	(60, 11, 7, NULL, 'Olá, TESTE! ueler teste', '2026-09-15 21:39:35', 0),
	(61, 11, 8, NULL, 'Olá, Uriel! ueler teste', '2026-09-15 21:39:35', 0),
	(62, 11, 10, NULL, 'Olá, Luiz! ueler teste', '2026-09-15 21:39:35', 0),
	(63, 12, 1, NULL, 'Olá, cliente! anuncio teste promocional', '2026-09-15 21:40:30', 1),
	(64, 12, 4, NULL, 'Olá, Teste! anuncio teste promocional', '2026-09-15 21:40:30', 0),
	(65, 12, 5, NULL, 'Olá, luiz! anuncio teste promocional', '2026-09-15 21:40:30', 0),
	(66, 12, 6, NULL, 'Olá, joao! anuncio teste promocional', '2026-09-15 21:40:30', 0),
	(67, 12, 7, NULL, 'Olá, TESTE! anuncio teste promocional', '2026-09-15 21:40:30', 0),
	(68, 12, 8, NULL, 'Olá, Uriel! anuncio teste promocional', '2026-09-15 21:40:30', 0),
	(69, 12, 10, NULL, 'Olá, Luiz! anuncio teste promocional', '2026-09-15 21:40:30', 0);

-- Copiando estrutura para tabela dbmatutos.servico
CREATE TABLE IF NOT EXISTS `servico` (
  `Codigo_Servico` int(11) NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(250) DEFAULT NULL,
  `Duracao` varchar(250) DEFAULT NULL,
  `Preco` decimal(10,2) DEFAULT NULL,
  `Tempo_Servico` int(11) DEFAULT NULL,
  `Ativo` tinyint(1) DEFAULT 1,
  PRIMARY KEY (`Codigo_Servico`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.servico: ~3 rows (aproximadamente)
INSERT INTO `servico` (`Codigo_Servico`, `Descricao`, `Duracao`, `Preco`, `Tempo_Servico`, `Ativo`) VALUES
	(1, 'CORTE SIMPLES 02', '20 Minutos', 25.00, 30, 1),
	(2, 'corte simples', '45 minutos', 45.00, 45, 1),
	(3, 'teste', '10', 10.00, 10, 1);

-- Copiando estrutura para tabela dbmatutos.servico_imagem
CREATE TABLE IF NOT EXISTS `servico_imagem` (
  `Codigo_Imagem` int(11) NOT NULL AUTO_INCREMENT,
  `Imagem` longtext DEFAULT NULL,
  `Codigo_Servico` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Imagem`),
  KEY `FK_servico_imagem_servico` (`Codigo_Servico`),
  CONSTRAINT `FK_servico_imagem_servico` FOREIGN KEY (`Codigo_Servico`) REFERENCES `servico` (`Codigo_Servico`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.servico_imagem: ~3 rows (aproximadamente)
INSERT INTO `servico_imagem` (`Codigo_Imagem`, `Imagem`, `Codigo_Servico`) VALUES
	(1, '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', 1),
	(3, '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', 2);

-- Copiando estrutura para tabela dbmatutos.situacao_agendamento
CREATE TABLE IF NOT EXISTS `situacao_agendamento` (
  `Codigo_Situacao_Agendamento` int(11) NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(250) DEFAULT NULL,
  PRIMARY KEY (`Codigo_Situacao_Agendamento`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.situacao_agendamento: ~4 rows (aproximadamente)
INSERT INTO `situacao_agendamento` (`Codigo_Situacao_Agendamento`, `Descricao`) VALUES
	(1, 'Aberto'),
	(2, 'Cancelado'),
	(3, 'Liberado'),
	(4, 'Concluido');

-- Copiando estrutura para tabela dbmatutos.telefone
CREATE TABLE IF NOT EXISTS `telefone` (
  `Codigo_Telefone` int(11) NOT NULL AUTO_INCREMENT,
  `Numero_Telefone` varchar(11) NOT NULL,
  `Principal` bit(1) NOT NULL,
  `DDD` char(2) DEFAULT NULL,
  PRIMARY KEY (`Codigo_Telefone`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.telefone: ~6 rows (aproximadamente)
INSERT INTO `telefone` (`Codigo_Telefone`, `Numero_Telefone`, `Principal`, `DDD`) VALUES
	(1, '981079115', b'1', '17'),
	(2, '981079116', b'1', '17'),
	(3, '999999999', b'1', '17'),
	(5, '1321321321', b'1', '12'),
	(6, '2134545453', b'0', '21'),
	(7, '1798079115', b'1', '17'),
	(9, '1231231231', b'0', '17');

-- Copiando estrutura para tabela dbmatutos.tipo_evento
CREATE TABLE IF NOT EXISTS `tipo_evento` (
  `Codigo_Tipo` int(11) NOT NULL AUTO_INCREMENT,
  `Nome` varchar(50) NOT NULL COMMENT 'Nome técnico: PreAgendamento, Inatividade, etc',
  `Descricao` varchar(150) DEFAULT NULL COMMENT 'Explicação para exibir na tela do admin',
  PRIMARY KEY (`Codigo_Tipo`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Copiando dados para a tabela dbmatutos.tipo_evento: ~3 rows (aproximadamente)
INSERT INTO `tipo_evento` (`Codigo_Tipo`, `Nome`, `Descricao`) VALUES
	(1, 'Pré-Agendamento', 'Envia um aviso X minutos antes do horário marcado.'),
	(2, 'Inatividade', 'Alerta clientes que estão há X dias sem realizar agendamentos.'),
	(3, 'Promocional', 'Disparos de mensagens gerais ou campanhas de marketing.');

-- Copiando estrutura para tabela dbmatutos.usuario
CREATE TABLE IF NOT EXISTS `usuario` (
  `Codigo_Usuario` int(11) NOT NULL AUTO_INCREMENT,
  `Nome` varchar(250) NOT NULL,
  `E_mail` varchar(250) NOT NULL,
  `Senha` varchar(250) NOT NULL,
  `Ativo` tinyint(1) DEFAULT 1,
  `Imagem_Usuario` varchar(250) DEFAULT NULL,
  `Codigo_Recuperacao` varchar(10) DEFAULT NULL,
  `Data_Validade_Codigo` datetime DEFAULT NULL,
  `TokenFCM` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Codigo_Usuario`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.usuario: ~11 rows (aproximadamente)
INSERT INTO `usuario` (`Codigo_Usuario`, `Nome`, `E_mail`, `Senha`, `Ativo`, `Imagem_Usuario`, `Codigo_Recuperacao`, `Data_Validade_Codigo`, `TokenFCM`) VALUES
	(1, 'cliente', 'uelerccb1698@gmail.com', '$2a$11$IrKKwtqb5lJ6AC9j5xTxUOiyJMK2JRt0xlgfGHAvvgybme0O5lqEm', 1, NULL, NULL, NULL, 'f7e1bnuiQn2wXGPf87vmSD:APA91bE7WsP9hW7IlbUrjYhBrsA4ZFVUEyrfHVSN78ZstKBpWPxlehIiKVHECVad1hfN_QTZiu6w5-JK-W3POpVKbrVr3DXSEIR5hhDdPfK1XXBygutqtdg'),
	(2, 'adm', 'adm', '$2a$11$.y7ljnOwNMKQMbqrTFzYY.bV3NLYuijVo2rqav0HJVVk2jVO5ppDK', 1, '/Uploads/perfil_2.jpg', NULL, NULL, 'e-s5KcBUQXy0Wva-8MwrcS:APA91bGYDNtEhG_G7QZGWI-J3974K0KHFm3rw6LTIKcLllatQYQdgeQJKxGfOWKsNrwnN1qwpj_L9Qxor0MU6IaxV9n8RUf9NG4kxIqan11A5ZoIjtnlrV4'),
	(3, 'cliente', 'ueler', '$2a$11$u7gptx0vtgoigbpWKXaQqef.3KVMBYKDsJXWhfmVU1P2JjnDTg.fK', 0, NULL, NULL, NULL, NULL),
	(4, 'Teste Ueler', 'teste@teste.com.br', '$2a$11$svPN.gDt6GoRntn1nMDTf.tOhNV.28clm2UrH8nLLtDolCV29/Ax2', 1, NULL, NULL, NULL, NULL),
	(5, 'luiz felipe da silva', 'PrincipalView', '$2a$11$OK8JoWRAPt7GZksvEoOHEuKM0Q8udJepHwL2FIaCiKupT6DIJqYLS', 1, NULL, NULL, NULL, NULL),
	(6, 'joao da silva', 'teste.teste.teste.com.br', '$2a$11$Osz4uM2LSekIy1z9I.NZqO15BTCF4KPzwjTpoGZnQU0GXPeryfyBu', 1, '/Uploads/perfil_6.jpg', NULL, NULL, NULL),
	(7, 'TESTE UELER BERNARDO', 'uelerccb1698@gmail.com', '$2a$11$FLcnV2suz54vAF7xkvmqnu74v4TI3wr8shO0.TmiqHr/QSub20D1S', 1, NULL, NULL, NULL, NULL),
	(8, 'Uriel Bernardo', 'teste@teste.com', '$2a$11$RzsK1iH20SMuxHSHgHR5YeanT3GJDIHHzdSHCTT1opJNqR2WddUCe', 1, NULL, NULL, NULL, NULL),
	(9, 'Barbeiro', 'barbeiro', '$2a$11$.y7ljnOwNMKQMbqrTFzYY.bV3NLYuijVo2rqav0HJVVk2jVO5ppDK', 1, NULL, NULL, NULL, NULL),
	(10, 'Luiz Felipe da Silva', 'luiz.silva12@aluno.unifafibe.edu.br', '$2a$11$LVv9VacIgZUOhNnHhKWMhudgzfspeZV21LwqZc9W1CR6gU9S7CIBS', 1, NULL, NULL, NULL, 'di48Do5WQuK4uM6FrwcIXf:APA91bGSOjnt9yD5QGxant6e_dzcB867P8azr7mA5S0rn6vNOVQnXZAIf5rRkg7G5I5P9q-KPVcoRHhUvnkhGSCm_C8UXidqUTDD1saWj7nfhrowh2LjSMw'),
	(11, 'BARBEIRO002', 'BARBEIRO@TESTE.COM', '$2a$11$Pnu5PyS7ZWUynl/6sr8nk.VSgA0CJc8pLAp8gyfhmjzeG1IEKAHs.', 1, NULL, NULL, NULL, NULL),
	(12, 'Uriel Bernardo', 'uriel.bernardo@gmail.com', '$2a$11$Wof2V1995kLsmAsVlg4UsuxxUlqZVZEcdXWJIEnjlSCE2/AOM/dEK', 1, NULL, NULL, NULL, NULL);

-- Copiando estrutura para tabela dbmatutos.usuario_blacklist
CREATE TABLE IF NOT EXISTS `usuario_blacklist` (
  `Codigo_Usuario_BlackList` int(11) NOT NULL AUTO_INCREMENT,
  `Codigo_Usuario` int(11) NOT NULL,
  `Codigo_BlackList` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario_BlackList`),
  KEY `Codigo_Usuario` (`Codigo_Usuario`),
  KEY `Codigo_BlackList` (`Codigo_BlackList`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `barbeiro` (`Codigo_Usuario`),
  CONSTRAINT `2` FOREIGN KEY (`Codigo_BlackList`) REFERENCES `blacklist` (`Codigo_Blacklist`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.usuario_blacklist: ~15 rows (aproximadamente)
INSERT INTO `usuario_blacklist` (`Codigo_Usuario_BlackList`, `Codigo_Usuario`, `Codigo_BlackList`) VALUES
	(1, 9, 1),
	(2, 9, 2),
	(3, 9, 3),
	(4, 9, 4),
	(5, 9, 5),
	(6, 9, 6),
	(7, 9, 7),
	(8, 9, 8),
	(9, 9, 9),
	(10, 9, 10),
	(11, 12, 11),
	(12, 11, 12),
	(13, 9, 13),
	(14, 11, 14),
	(15, 11, 15);

-- Copiando estrutura para tabela dbmatutos.usuario_telefone
CREATE TABLE IF NOT EXISTS `usuario_telefone` (
  `Codigo_Usuario_Telefone` int(11) NOT NULL AUTO_INCREMENT,
  `Codigo_Usuario` int(11) NOT NULL,
  `Codigo_Telefone` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario_Telefone`),
  KEY `Codigo_Usuario` (`Codigo_Usuario`),
  KEY `Codigo_Telefone` (`Codigo_Telefone`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`),
  CONSTRAINT `2` FOREIGN KEY (`Codigo_Telefone`) REFERENCES `telefone` (`Codigo_Telefone`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Copiando dados para a tabela dbmatutos.usuario_telefone: ~6 rows (aproximadamente)
INSERT INTO `usuario_telefone` (`Codigo_Usuario_Telefone`, `Codigo_Usuario`, `Codigo_Telefone`) VALUES
	(1, 1, 1),
	(2, 4, 2),
	(3, 5, 3),
	(5, 6, 5),
	(6, 6, 6),
	(7, 7, 7),
	(9, 2, 9);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
