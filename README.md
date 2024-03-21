# Render Pipeline Graph

这个项目是测试Unity正在推广的Render graph用的，可以将各个pass、资源Handle节点可视化编辑，并统计运行时间。

本人会用这个来测试一些渲染算法，完成后整合到现有管线中。

本项目是个人项目，因为要校招就开源出来，希望能给简历加分。

本项目参考了shader graph、VFX Graph的UI设计，使用了shader graph部分代码完成持久化，所以应该遵守Unity的许可证[Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License)

## Unity版本
对应Unity版本：目前适配了2023.3.0b10。因为非稳定版本的代码是天天改的，尤其是还在完善的功能（光是API就重命名了很多次），所以这个项目用其他版本几乎肯定报错。我在本项目完成到比较满意后，有时间会适配新的alpha、beta版本

## 项目来源

本项目受到[【图程回收站】Unity SRP自定义渲染管线demo演示](https://www.bilibili.com/video/BV1rj41117hy)启发。

## 开发日志
### 2024年3月22日：
- 支持Native Render Pass

### 2024年3月17日：
- 帧间保留的Texture List可以用了
- 加上了简陋的TAA

### 2024年3月9日：
- 适配2023.3.0b10
- Unity终于把天空盒RenderList的API加进RenderGraph了，RenderGraph的pass用自带的天空盒了，天空不再漆黑一片

