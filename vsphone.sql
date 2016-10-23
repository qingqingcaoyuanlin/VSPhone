/*
Navicat MySQL Data Transfer

Source Server         : vsphone
Source Server Version : 60011
Source Host           : localhost:3306
Source Database       : vsphone

Target Server Type    : MYSQL
Target Server Version : 60011
File Encoding         : 65001

Date: 2016-10-22 12:42:27
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `device`
-- ----------------------------
DROP TABLE IF EXISTS `device`;
CREATE TABLE `device` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `ProjectCode` char(20) NOT NULL DEFAULT '',
  `Header` char(60) DEFAULT NULL,
  `DeviceType` char(20) DEFAULT NULL,
  `DeviceNum` char(20) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of device
-- ----------------------------

-- ----------------------------
-- Table structure for `record`
-- ----------------------------
DROP TABLE IF EXISTS `record`;
CREATE TABLE `record` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `ProjectCode` char(20) DEFAULT '',
  `Header` char(60) DEFAULT NULL,
  `DeviceType` char(20) DEFAULT NULL,
  `DeviceNum` char(20) DEFAULT NULL,
  `Time` datetime DEFAULT NULL,
  `SetPeriod` int(1) DEFAULT NULL,
  `CallPeriod` int(1) DEFAULT NULL,
  `Succeed` bit(1) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of record
-- ----------------------------
