﻿namespace Atata
{
    public class FieldVerificationProvider<TData, TField, TOwner> :
        UIComponentVerificationProvider<TField, FieldVerificationProvider<TData, TField, TOwner>, TOwner>,
        IDataVerificationProvider<TData, TOwner>
        where TField : Field<TData, TOwner>
        where TOwner : PageObject<TOwner>
    {
        public FieldVerificationProvider(TField control)
            : base(control)
        {
        }

        public new NegationFieldVerificationProvider Not => new NegationFieldVerificationProvider(Component);

        IDataProvider<TData, TOwner> IDataVerificationProvider<TData, TOwner>.DataProvider => Component;

        public class NegationFieldVerificationProvider
            : NegationControlVerificationProvider, IDataVerificationProvider<TData, TOwner>
        {
            public NegationFieldVerificationProvider(TField control)
                : base(control)
            {
            }

            IDataProvider<TData, TOwner> IDataVerificationProvider<TData, TOwner>.DataProvider => Component;
        }
    }
}
