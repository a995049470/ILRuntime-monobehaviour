先搞一个简单的测试说明 详细的下次

1.热更脚本 在 HotFix 下创建
2.如果 该类需要能在Unity的面板种赋值  请打上 [ILRSerializable] 标签
3.如果 该类需要继承 MonoBehaviour 请改为 继承 BaseMono 然后就可以使用 Awake 等 生命周期函数

( 以下这些我都加进模板里 可以和原来一样调用 Awake,Start;Update;FixedUpdate;LateUpdate;OnEnable;OnDisable;
OnDestroy;OnTriggerEnter;OnTriggerStay;OnTriggerExit;
OnCollisionEnter;OnCollisionStay;OnCollisionExit)

一些问题:不支持面板对泛型参数为热更类的对象赋值  只有子物体的 GameObject 和 Component 类的 可以通过面板赋值 其他赋值会在关闭UNITY 后出错

4.如果 你需要一个方法在生成的中间脚本里也有 请打上 [ILRMonoMethod] 标签
5.如果 一个生命周期函数没有如预期调用(其实是我没加到模板里) 打上[ILRMonoMethod] 就可以正常调用

完成热更脚本编写后:

1.点击 Tools/MyTool/生成热更类中间脚本 然后就可以 点击物体的 Add Component 搜索 "ILR_ " + 热更类名 添加脚本
2.再进行面板赋值之后 要点击面板上的保存数据按钮 就会将数据保存成JSON 再运行时 通过反射赋值给热更类