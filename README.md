# README

## 参数

机械臂ID：在ScaraRobotArm.cs文件中，新机械臂ID=19，老机械臂ID=82

频谱仪IP地址：192.168.1.51

## 蛇形扫描部分

* 扫描顺序是：从设定的start到stop逐点扫描。

  ```c#
  //步骤2：计算当前点位的目标坐标
                          float targetX = scanSettings.StartX + i * (scanSettings.StopX - scanSettings.StartX) / (scanSettings.NumX - 1);
                          float targetY = scanSettings.StartY + j * (scanSettings.StopY - scanSettings.StartY) / (scanSettings.NumY - 1);
  
                          //步骤3：移动机械臂到目标点位
                          await _hardwareService.ActiveRobot.MoveToAsync(targetX, targetY, scanSettings.ScanHeightZ, scanSettings.ScanAngleR);
  ```

* 移动机械臂的函数进行了更新，MoveToAsync(targetX, targetY, scanSettings.ScanHeightZ, scanSettings.ScanAngleR)