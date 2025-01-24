using System;
using System.Collections.Generic;
using OoLunar.Tomoe.Interactivity.ComponentCreators;
using OoLunar.Tomoe.Interactivity.Moments.Choose;
using OoLunar.Tomoe.Interactivity.Moments.Confirm;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;
using OoLunar.Tomoe.Interactivity.Moments.Pick;
using OoLunar.Tomoe.Interactivity.Moments.Prompt;

namespace OoLunar.Tomoe.Interactivity
{
    public sealed record ProcrastinatorConfiguration
    {
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(30);
        public Dictionary<Type, IComponentCreator> ComponentCreators { get; init; } = new()
        {
            { typeof(IChooseComponentCreator), new ChooseDefaultComponentCreator() },
            { typeof(IConfirmComponentCreator), new ConfirmDefaultComponentCreator() },
            { typeof(IPaginationComponentCreator), new PaginationDefaultComponentCreator() },
            { typeof(IPickComponentCreator), new PickDefaultComponentCreator() },
            { typeof(IPromptComponentCreator), new PromptDefaultComponentCreator() }
        };

        public TComponentCreator GetComponentCreatorOrDefault<TComponentCreator, TDefaultComponentCreator>()
            where TComponentCreator : IComponentCreator
            where TDefaultComponentCreator : TComponentCreator, new()
                => ComponentCreators.TryGetValue(typeof(TComponentCreator), out IComponentCreator? creator)
                    ? (TComponentCreator)creator
                    : new TDefaultComponentCreator();
    }
}
