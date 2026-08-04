/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Criação da tabela usuario
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
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela administrador
CREATE TABLE IF NOT EXISTS `administrador` (
  `Codigo_Usuario` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela barbeiro
CREATE TABLE IF NOT EXISTS `barbeiro` (
  `Codigo_Usuario` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela cliente
CREATE TABLE IF NOT EXISTS `cliente` (
  `Codigo_Usuario` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela situacao_agendamento
CREATE TABLE IF NOT EXISTS `situacao_agendamento` (
  `Codigo_Situacao_Agendamento` int(11) NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(250) DEFAULT NULL,
  PRIMARY KEY (`Codigo_Situacao_Agendamento`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela agendamento
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
) ENGINE=InnoDB AUTO_INCREMENT=91 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela servico
CREATE TABLE IF NOT EXISTS `servico` (
  `Codigo_Servico` int(11) NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(250) DEFAULT NULL,
  `Duracao` varchar(250) DEFAULT NULL,
  `Preco` decimal(10,2) DEFAULT NULL,
  `Tempo_Servico` int(11) DEFAULT NULL,
  `Ativo` tinyint(1) DEFAULT 1,
  PRIMARY KEY (`Codigo_Servico`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela agendamento_servico
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
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela auditoria_log
CREATE TABLE IF NOT EXISTS `auditoria_log` (
  `Codigo_Log` int(11) NOT NULL AUTO_INCREMENT,
  `Entidade_Afetada` varchar(250) DEFAULT NULL,
  `Tipo_Acao` int(11) DEFAULT NULL,
  `Valor_Afetado` int(11) DEFAULT NULL,
  `Usuario_Acao` varchar(250) DEFAULT NULL,
  `Ativo` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`Codigo_Log`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela blacklist
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
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela tipo_evento
CREATE TABLE IF NOT EXISTS `tipo_evento` (
  `Codigo_Tipo` int(11) NOT NULL AUTO_INCREMENT,
  `Nome` varchar(50) NOT NULL COMMENT 'Nome técnico: PreAgendamento, Inatividade, etc',
  `Descricao` varchar(150) DEFAULT NULL COMMENT 'Explicação para exibir na tela do admin',
  PRIMARY KEY (`Codigo_Tipo`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criação da tabela configura_notificacao
CREATE TABLE IF NOT EXISTS `configura_notificacao` (
  `Codigo_Notificacao` int(11) NOT NULL AUTO_INCREMENT,
  `Ativo` tinyint(1) DEFAULT 1,
  `Codigo_Tipo` int(11) NOT NULL COMMENT 'Chave Estrangeira ligada à tabela Tipo_Evento',
  `Descricao` varchar(250) DEFAULT NULL,
  `Mensagem` varchar(250) DEFAULT NULL,
  `Valor` int(11) DEFAULT NULL,
  `UnidadeTempo` int(11) DEFAULT NULL COMMENT '1 = Minutos, 2 = Horas, 3 = Dias',
  PRIMARY KEY (`Codigo_Notificacao`),
  KEY `FK_Configura_TipoEvento` (`Codigo_Tipo`),
  CONSTRAINT `FK_Configura_TipoEvento` FOREIGN KEY (`Codigo_Tipo`) REFERENCES `tipo_evento` (`Codigo_Tipo`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criação da tabela notificacao
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
) ENGINE=InnoDB AUTO_INCREMENT=87 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criação da tabela servico_imagem
CREATE TABLE IF NOT EXISTS `servico_imagem` (
  `Codigo_Imagem` int(11) NOT NULL AUTO_INCREMENT,
  `Imagem` longtext DEFAULT NULL,
  `Codigo_Servico` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Imagem`),
  KEY `FK_servico_imagem_servico` (`Codigo_Servico`),
  CONSTRAINT `FK_servico_imagem_servico` FOREIGN KEY (`Codigo_Servico`) REFERENCES `servico` (`Codigo_Servico`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela telefone
CREATE TABLE IF NOT EXISTS `telefone` (
  `Codigo_Telefone` int(11) NOT NULL AUTO_INCREMENT,
  `Numero_Telefone` varchar(11) NOT NULL,
  `Principal` bit(1) NOT NULL,
  `DDD` char(2) DEFAULT NULL,
  PRIMARY KEY (`Codigo_Telefone`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela usuario_blacklist
CREATE TABLE IF NOT EXISTS `usuario_blacklist` (
  `Codigo_Usuario_BlackList` int(11) NOT NULL AUTO_INCREMENT,
  `Codigo_Usuario` int(11) NOT NULL,
  `Codigo_BlackList` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario_BlackList`),
  KEY `Codigo_Usuario` (`Codigo_Usuario`),
  KEY `Codigo_BlackList` (`Codigo_BlackList`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `barbeiro` (`Codigo_Usuario`),
  CONSTRAINT `2` FOREIGN KEY (`Codigo_BlackList`) REFERENCES `blacklist` (`Codigo_Blacklist`)
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Criação da tabela usuario_telefone
CREATE TABLE IF NOT EXISTS `usuario_telefone` (
  `Codigo_Usuario_Telefone` int(11) NOT NULL AUTO_INCREMENT,
  `Codigo_Usuario` int(11) NOT NULL,
  `Codigo_Telefone` int(11) NOT NULL,
  PRIMARY KEY (`Codigo_Usuario_Telefone`),
  KEY `Codigo_Usuario` (`Codigo_Usuario`),
  KEY `Codigo_Telefone` (`Codigo_Telefone`),
  CONSTRAINT `1` FOREIGN KEY (`Codigo_Usuario`) REFERENCES `usuario` (`Codigo_Usuario`),
  CONSTRAINT `2` FOREIGN KEY (`Codigo_Telefone`) REFERENCES `telefone` (`Codigo_Telefone`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;s