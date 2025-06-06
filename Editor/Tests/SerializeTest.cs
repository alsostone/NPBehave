using MemoryPack;
using NUnit.Framework;

namespace NPBehave
{
    public class SerializeTest
    {
        [Test]
        public void ShouldEqual_SerializedBehaveWorld()
        {
            var behaveWorld = new BehaveWorld();
            var sharedBlackboard = behaveWorld.GetSharedBlackboard("example-swarm-ai");
            sharedBlackboard.SetBool("sharedBlackboard", true);
            
            var blackboard1 = behaveWorld.CreateBlackboard(sharedBlackboard);
            var blackboard2 = behaveWorld.CreateBlackboard(sharedBlackboard);
            var blackboard3 = behaveWorld.CreateBlackboard(sharedBlackboard);
            var blackboard4 = behaveWorld.CreateBlackboard(sharedBlackboard);

            var blackboard1Guid = blackboard1.Guid;
            var blackboard2Guid = blackboard2.Guid;
            var blackboard3Guid = blackboard3.Guid;
            var blackboard4Guid = blackboard4.Guid;
            
            blackboard1.SetBool("blackboard1", true);
            blackboard2.SetInt("blackboard2", 100);
            blackboard3.SetBool("blackboard3", true);
            blackboard4.SetBool("blackboard4", true);
            
            var bytes = MemoryPackSerializer.Serialize(behaveWorld);
            var behaveWorld1 = MemoryPackSerializer.Deserialize<BehaveWorld>(bytes);
            var sharedBlackboardNew = behaveWorld.GetSharedBlackboard("example-swarm-ai");

            var blackboard1New = behaveWorld1.GetBlackboard(blackboard1Guid);
            var blackboard2New = behaveWorld1.GetBlackboard(blackboard2Guid);
            var blackboard3New = behaveWorld1.GetBlackboard(blackboard3Guid);
            var blackboard4New = behaveWorld1.GetBlackboard(blackboard4Guid);
            
            Assert.AreEqual(blackboard1.GetBool("blackboard1"), blackboard1New.GetBool("blackboard1"));
            Assert.AreEqual(blackboard2.GetInt("blackboard2"), blackboard2New.GetInt("blackboard2"));
            Assert.AreEqual(blackboard3.GetBool("blackboard3"), blackboard3New.GetBool("blackboard3"));
            Assert.AreEqual(blackboard4.GetBool("blackboard4"), blackboard4New.GetBool("blackboard4"));
            
            Assert.AreEqual(sharedBlackboard.GetBool("sharedBlackboard"), sharedBlackboardNew.GetBool("sharedBlackboard"));
            Assert.AreEqual(behaveWorld.Clock.NumTimers, 5);
            Assert.AreEqual(behaveWorld1.Clock.NumTimers, 5);
            
            behaveWorld.Update(1f);
            behaveWorld1.Update(1f);
            
            Assert.AreEqual(behaveWorld.Clock.NumTimers, 0);
            Assert.AreEqual(behaveWorld1.Clock.NumTimers, 0);
        }
    }
}