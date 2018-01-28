﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using ProjectRimFactory.Storage.Editables;

namespace ProjectRimFactory.Storage
{
    public abstract class Building_MassStorageUnit : Building_Storage
    {
        List<Thing> items = new List<Thing>();
        public abstract bool CanStoreMoreItems { get; }
        public IEnumerable<Thing> StoredItems => items;
        public int StoredItemsCount => items.Count;

        public override void Notify_ReceivedThing(Thing newItem)
        {
            base.Notify_ReceivedThing(newItem);
            if (newItem.Position != Position)
            {
                RegisterNewItem(newItem);
            }
            if (newItem.def.drawGUIOverlay)
            {
                Map.listerThings.ThingsInGroup(ThingRequestGroup.HasGUIOverlay).Remove(newItem);
            }
        }

        public virtual string GetITabString()
        {
            return "PRFItemsTabLabel".Translate(items.Count);
        }

        protected virtual void RegisterNewItem(Thing newItem)
        {
            List<Thing> things = Position.GetThingList(Map);
            for (int i = 0; i < things.Count; i++)
            {
                Thing item = things[i];
                if (item.def.category == ThingCategory.Item && item.CanStackWith(newItem))
                {
                    item.TryAbsorbStack(newItem, true);
                }
                if (newItem.Destroyed)
                {
                    break;
                }
            }
            if (!newItem.Destroyed)
            {
                items.Add(newItem);
                if (CanStoreMoreItems)
                {
                    newItem.Position = Position;
                }
            }
        }
        public override string GetInspectString()
        {
            string original = base.GetInspectString();
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(original))
            {
                stringBuilder.AppendLine(original);
            }
            stringBuilder.Append("PRF_TotalStacksNum".Translate(items.Count));
            return stringBuilder.ToString();
        }
        public override void DeSpawn()
        {
            List<Thing> thingsToSplurge = new List<Thing>(Position.GetThingList(Map));
            for (int i = 0; i < thingsToSplurge.Count; i++)
            {
                if (thingsToSplurge[i].def.category == ThingCategory.Item)
                {
                    thingsToSplurge[i].DeSpawn();
                    GenPlace.TryPlaceThing(thingsToSplurge[i], Position, Map, ThingPlaceMode.Near);
                }
            }
            base.DeSpawn();
        }
        public override void Notify_LostThing(Thing newItem)
        {
            base.Notify_LostThing(newItem);
            items.Remove(newItem);
            RefreshStorage();
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            RefreshStorage();
        }
        protected virtual void RefreshStorage()
        {
            items = new List<Thing>();
            foreach (IntVec3 cell in AllSlotCells())
            {
                List<Thing> things = new List<Thing>(cell.GetThingList(Map));
                for (int i = 0; i < things.Count; i++)
                {
                    Thing item = things[i];
                    if (item.def.category == ThingCategory.Item)
                    {
                        if (cell != Position)
                        {
                            RegisterNewItem(item);
                        }
                        else
                        {
                            items.Add(item);
                        }
                        if (item.def.drawGUIOverlay)
                        {
                            Map.listerThings.ThingsInGroup(ThingRequestGroup.HasGUIOverlay).Remove(item);
                        }
                    }
                }
            }
        }
    }
}
