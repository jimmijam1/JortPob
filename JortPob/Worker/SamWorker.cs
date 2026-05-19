using JortPob.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace JortPob.Worker
{
    // To be reworked with SAM alt generation rework
    public class SamWorker : IWorker<Unit>
    {
        private readonly List<SoundManager.SAMData> datas;

        public SamWorker(List<SoundManager.SAMData> datas)
        {
            this.datas = datas;
        }
        private readonly int start;
        private readonly int end;

        public SamWorker(List<SoundManager.SAMData> datas, int start, int end)
        {
            this.datas = datas;
            this.start = start;
            this.end = end;
        }

        private void Run()
        {
            for(int i = start;i<Math.Min(datas.Count(), end);i++)
            {
                SoundManager.SAMData dat = datas[i];
                SAM.GenerateAlt(dat.dialog, dat.info, dat.line, dat.hashName, dat.npc);
                Lort.TaskIterate(); // Progress bar update
            }
        }

        public Unit Go()
        {
            Lort.Log($"Generating {datas.Count()} WEMs...", Lort.Type.Main);
            Lort.NewTask("Writing WEMs", datas.Count);

            int partition = (int)Math.Ceiling(datas.Count / (float)Const.THREAD_COUNT);
            List<SamWorker> workers = new();

            datas.AsParallel()
                .WithDegreeOfParallelism(Const.THREAD_COUNT)
                .ForAll(data =>
                {
                    SAM.GenerateAlt(data.dialog, data.info, data.line, data.hashName, data.npc);
                    Lort.TaskIterate();
                    return;
                });

            return Unit.Default;
        }
    }
}
