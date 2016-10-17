SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `device`
-- ----------------------------
DROP TABLE IF EXISTS `device`;
CREATE TABLE `device` (
  `DeviceNum` char(20) DEFAULT NULL,
  `DeviceType` char(20) DEFAULT NULL,
  `Header` char(60) DEFAULT NULL,
  `ProjectCode` char(20) DEFAULT NULL,
  PRIMARY KEY  (`ProjectCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of device
-- ----------------------------

-- ----------------------------
-- Table structure for `record`
-- ----------------------------
DROP TABLE IF EXISTS `record`;
CREATE TABLE `record` (
  `ProjectCode` char(20) DEFAULT NULL,
  `Header` char(60) DEFAULT NULL,
  `DeviceType` char(20) DEFAULT NULL,
  `DeviceNum` char(20) DEFAULT NULL,
  `Time` datetime DEFAULT NULL,
  `SetPeriod` int(1) DEFAULT NULL,
  `CallPeriod` int(1) DEFAULT NULL,
  `Succeed` bit(1) DEFAULT NULL,  
  `deviceaa` int(1) DEFAULT NULL,
  PRIMARY KEY  (`ProjectCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of record
-- ----------------------------
