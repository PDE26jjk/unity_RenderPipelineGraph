# Render Pipeline Graph

<img src="https://raw.githubusercontent.com/PDE26jjk/misc/main/img/image-20240325174108440.png" alt="image-20240325174108440"  />

这个项目是测试Unity正在推广的Render graph用的，可以将各个pass、资源Handle节点可视化编辑，并统计运行时间。

本人会用这个来测试一些渲染算法，完成后整合到现有管线中。

本项目是个人项目，因为要校招就开源出来，希望能给简历加分。（然而并没有找到工作

本项目参考了shader graph、VFX Graph的UI设计，使用了shader graph部分代码完成持久化，所以应该遵守Unity的许可证[Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License)

## Unity版本
对应Unity版本：目前适配了2023.3.0b10。因为Unity的非稳定版本的代码是频繁更改的，尤其是还在完善的功能（光是Render Graph的API就被重命名了很多次），所以这个项目用其他版本几乎肯定报错。我在将本项目完成到比较满意后，有时间会适配新的alpha、beta版本



## 项目来源

本项目受到[【图程回收站】Unity SRP自定义渲染管线demo演示](https://www.bilibili.com/video/BV1rj41117hy)启发。

## 待办列表

- [ ] 将获取Pass的反射改成Roslyn自动生成代码。
- [ ] 完善界面，将统计的运行时间显示到节点上。
- [ ] 加入更多的常见管线实现代码。

## 开发日志

### 2024年3月22日：
- 支持Native Render Pass

- 点Render Graph Viewer下面这个按钮可以跳转到对应Pass的cs文件了，不过要在派生的pass中主动调用父类构造函数才能用

  <img src="https://raw.githubusercontent.com/PDE26jjk/misc/main/img/image-20240322024016604.png" alt="image-20240322024016604" style="zoom:67%;" />

### 2024年3月17日：
- 帧间保留的Texture List可以用了
- 加上了简陋的TAA

### 2024年3月9日：
- 适配2023.3.0b10
- Unity终于把天空盒RenderList的API加进RenderGraph了，RenderGraph的pass用自带的天空盒了，天空不再漆黑一片

