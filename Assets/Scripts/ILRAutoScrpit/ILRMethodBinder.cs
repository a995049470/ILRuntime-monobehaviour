public class ILRMethodBinder 
{
	public static void MethodBinder()
	{
#if ILRuntime
#else
		ILR_BaseMono.GetMothodOnInstantiate();
		ILR_T1.GetMothodOnInstantiate();

#endif
	}
}

