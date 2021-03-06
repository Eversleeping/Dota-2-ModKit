﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using KVLib;

namespace D2ModKit
{
    public class Addon
    {
        private string name;
        private string contentPath;
        private string gamePath;
        private string npcPath;
        private string copyPath;
        private string addonEnglishPath;
        private string abilitiesCustomPath;
        private string itemCustomPath;
        private string unitsCustomPath;
        private string heroesCustomPath;
        private string relativeParticlePath;
		private string gds_modID = "";
		private string gds_rank = "";

        private List<AbilityEntry> abilityEntries = new List<AbilityEntry>();
        private List<AbilityEntry> itemEntries = new List<AbilityEntry>();
        private List<UnitEntry> unitEntries = new List<UnitEntry>();
        private List<HeroEntry> heroesEntries = new List<HeroEntry>();
        // HashSet ensures no duplicate modifier entries.
        private HashSet<string> modifierItemKeys = new HashSet<string>();
        private HashSet<string> modifierAbilityKeys = new HashSet<string>();
        private HashSet<string> hiddenModifierKeys = new HashSet<string>();

        // for storing the addon_language kvs
        private HashSet<string> alreadyHasKeys = new HashSet<string>();

        public List<AbilityEntry> AbilityEntries
        {
            get { return abilityEntries; }
            set { abilityEntries = value; }
        }

        private List<string> particlePaths;

        private void getMorePaths()
        {
            AddonEnglishPath = Path.Combine(GamePath, "resource", "addon_english.txt");
            AbilitiesCustomPath = Path.Combine(GamePath, "scripts", "npc", "npc_abilities_custom.txt");
            ItemsCustomPath = Path.Combine(GamePath, "scripts", "npc", "npc_items_custom.txt");
            UnitsCustomPath = Path.Combine(GamePath, "scripts", "npc", "npc_units_custom.txt");
            HeroesCustomPath = Path.Combine(GamePath, "scripts", "npc", "npc_heroes_custom.txt");
        }

        public Addon(string _gamePath)
        {
            gamePath = _gamePath;
            name = gamePath.Substring(gamePath.LastIndexOf('\\') + 1);
            getMorePaths();
        }

        public Addon(string _contentPath, string _gamePath)
        {
            contentPath = _contentPath;
            gamePath = _gamePath;
            name = _contentPath.Substring(_contentPath.LastIndexOf('\\') + 1);
            getMorePaths();
        }

		public string GDS_modID {
			get { return gds_modID; }
			set { gds_modID = value; }
		}

		public string GDS_rank {
			get { return gds_rank; }
			set { gds_rank = value; }
		}

        public string ItemsCustomPath
        {
            get { return itemCustomPath; }
            set { itemCustomPath = value; }
        }

        public string UnitsCustomPath
        {
            get { return unitsCustomPath; }
            set { unitsCustomPath = value; }
        }

        public string HeroesCustomPath
        {
            get { return heroesCustomPath; }
            set { heroesCustomPath = value; }
        }

        public string AddonEnglishPath
        {
            get { return addonEnglishPath; }
            set { addonEnglishPath = value; }
        }

        public string AbilitiesCustomPath
        {
            get { return abilitiesCustomPath; }
            set { abilitiesCustomPath = value; }
        }

        public string RelativeParticlePath
        {
            get { return relativeParticlePath; }
            set { relativeParticlePath = value; }
        }

        public string CopyPath
        {
            get { return copyPath; }
            set { copyPath = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string ContentPath
        {
            get { return contentPath; }
            set { contentPath = value; }
        }

        public string GamePath
        {
            get { return gamePath; }
            set { gamePath = value; }
        }

        public string NPCPath
        {
            get { return npcPath; }
            set { npcPath = value; }
        }

        public List<string> ParticlePaths
        {
            get { return particlePaths; }
            set { particlePaths = value; }
        }
		public HashSet<swf> swfs = new HashSet<swf>();

		public class swf {
			public string as3Path;
			public string swfPath;
			public string as3Contents;
			public int currNum;
			public int newNum;
			public string name;
			public string nameWithoutNumber;
			public bool hasNum = false;
			public bool wasChecked = false;
			public string prevName;
			public string newSwfPath;
			public string fla;

			public swf(string swfPath) {
				this.swfPath = swfPath;
				name = swfPath.Substring(swfPath.LastIndexOf('\\') + 1);
				name = name.Replace(".swf", "");
				nameWithoutNumber = name;

				// get the number
				if (name.Contains("_")) {
					if (int.TryParse(name.Substring(name.LastIndexOf("_") + 1), out currNum)) {
						hasNum = true;
						newNum = currNum + 1;
						nameWithoutNumber = name.Substring(0, name.LastIndexOf("_"));
					}
				}
			}

			internal void changeCurrNum(int p) {
				currNum = p;
				newNum = p + 1;
			}
		}

		public void addSwf(swf swf) {
			this.swfs.Add(swf);
		}

		public List<CombineKVFile> combineKVFiles = new List<CombineKVFile>();

		public class CombineKVFile {
			public string path;
			public bool activated;
			public string name;
			public CombineKVFile(string name) {
				this.name = name;
			}

		}

		public void getPreferences() {
			KeyValue pref = null;
			KeyValue kv_files = null;
			foreach (KeyValue kv in KVData.Children) {
				if (kv.Key == "preferences") {
					pref = kv;
				}
			}

			foreach (KeyValue kv in pref.Children) {
				if (kv.Key == "create_note0_lore") {
					if (kv.Children.ElementAt(0).Key == "1") {
						create_note0_lore = true;
					} else {
						create_note0_lore = false;
					}
				} else if (kv.Key == "kv_files") {
					kv_files = kv;
				}
			}

			foreach (KeyValue kv in kv_files.Children) {
				CombineKVFile cf = new CombineKVFile(kv.Key);
				foreach (KeyValue kv2 in kv.Children) {
					if (kv2.Key == "path") {
						cf.path = kv2.Children.ElementAt(0).Key;
					}
					if (kv2.Key == "activated") {
						if (kv2.Children.ElementAt(0).Key == "1") {
							cf.activated = true;
						} else {
							cf.activated = false;
						}
					}
				}
				combineKVFiles.Add(cf);
			}
		}

        public void getCurrentAddonEnglish()
        {
            // Parse addon_english.txt KV
            KeyValue[] addonEnglishKeyVals = KVParser.KV1.ParseAll(File.ReadAllText(addonEnglishPath));
        }

        public void getAbilityTooltips(bool items)
        {
            if (items)
            {
                itemEntries.Clear();
                modifierItemKeys.Clear();

                if (!File.Exists(itemCustomPath))
                {
                    return;
                }
            }
            else
            {
                AbilityEntries.Clear();
                modifierAbilityKeys.Clear();

                if (!File.Exists(abilitiesCustomPath))
                {
                    return;
                }
            }

            // Parse abilities_custom.txt KV

            KeyValue[] abilitiesCustomKeyVals = KVParser.KV1.ParseAll(File.ReadAllText(AbilitiesCustomPath));
            if (items)
            {
                abilitiesCustomKeyVals = KVParser.KV1.ParseAll(File.ReadAllText(ItemsCustomPath));
            }

            IEnumerable<KeyValue> abilityNames = null;
            try
            {
                abilityNames = abilitiesCustomKeyVals[0].Children;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }

            for (int i = 0; i < abilityNames.Count(); i++)
            {
                KeyValue ability = abilityNames.ElementAt(i);
                if (ability.Key == "Version")
                {
                    continue;
                }
                if (ability.HasChildren)
                {
                    // added will remain false if ability has no AbilitySpecials.
                    bool added = false;

                    IEnumerable<KeyValue> children = ability.Children;
                    // Find the abilityspecial stuff.
                    for (int j = 0; j < children.Count(); j++)
                    {
                        KeyValue child = children.ElementAt(j);
                        if (child.Key == "AbilitySpecial" || child.Key == "Modifiers")
                        {
                            bool modifiers = false;
                            if (child.Key == "Modifiers")
                            {
                                modifiers = true;
                            }

                            // We have the AbilitySpecial now. See if there is actually anything in it.
                            if (child.HasChildren)
                            {
                                List<string> kvs = new List<string>();
                                IEnumerable<KeyValue> children2 = child.Children;
                                for (int k = 0; k < children2.Count(); k++)
                                {
                                    KeyValue child2 = children2.ElementAt(k);
                                    bool isHidden = false;
                                    if (child2.HasChildren)
                                    {
                                        IEnumerable<KeyValue> children3 = child2.Children;
                                        for (int l = 0; l < children3.Count(); l++)
                                        {
                                            KeyValue child3 = children3.ElementAt(l);
                                            if (modifiers)
                                            {
                                                if (child3.Key == "IsHidden")
                                                {
                                                    // Ensure it's actually hidden.
                                                    if (child3.GetString() == "1")
                                                    {
                                                        isHidden = true;
                                                    }
                                                }
                                            }
                                            else // we have a modifier, not ability.
                                            {
                                                // Map item modName to its item specials.
                                                if (child3.Key != "var_type")
                                                {
                                                    kvs.Add(child3.Key);
                                                }
                                            }
                                        }
                                    }
                                    // we're done going through all the rootChildren of this ability/modifier.
                                    if (modifiers)
                                    {
                                        if (!isHidden)
                                        {
                                            if (items)
                                            {
                                                if (!modifierItemKeys.Contains(child2.Key))
                                                {
                                                    modifierItemKeys.Add(child2.Key);
                                                }
                                            }
                                            else
                                            {
                                                if (!modifierAbilityKeys.Contains(child2.Key))
                                                {
                                                    modifierAbilityKeys.Add(child2.Key);
                                                }
                                            }
                                            added = true;
                                        }
                                    }
                                }
                                if (!modifiers)
                                {
                                    if (items)
                                    {
                                        itemEntries.Add(new AbilityEntry(ability.Key, kvs));
                                    }
                                    else
                                    {
                                        abilityEntries.Add(new AbilityEntry(ability.Key, kvs));
                                    }
                                    added = true;
                                }
                            }
                        }
                    }
                    // this ability has no AbilitySpecials.
                    if (!added)
                    {
                        if (items)
                        {
                            itemEntries.Add(new AbilityEntry(ability.Key, null));
                        }
                        else
                        {
                            abilityEntries.Add(new AbilityEntry(ability.Key, null));
                        }
                    }
                }
            }
        }

        public void getHeroesTooltips()
        {
            heroesEntries.Clear();

            if (!File.Exists(heroesCustomPath))
            {
                return;
            }

            KeyValue[] heroesKeyVals = KVParser.KV1.ParseAll(File.ReadAllText(heroesCustomPath));
            IEnumerable<KeyValue> children = heroesKeyVals[0].Children;
            for (int i = 0; i < children.Count(); i++)
            {
                KeyValue child = children.ElementAt(i);
                if (child.HasChildren)
                {
                    IEnumerable<KeyValue> children2 = child.Children;
                    for (int j = 0; j < children2.Count(); j++)
                    {
                        KeyValue child2 = children2.ElementAt(j);
                        if (child2.Key == "override_hero")
                        {
                            heroesEntries.Add(new HeroEntry(child2.GetString()));
                        }
                    }
                }
            }
        }

        public void getUnitTooltips()
        {
            unitEntries.Clear();

            if (!File.Exists(unitsCustomPath))
            {
                return;
            }

            KeyValue[] unitsKeyVals = KVParser.KV1.ParseAll(File.ReadAllText(unitsCustomPath));
            IEnumerable<KeyValue> children = unitsKeyVals[0].Children;
            for (int i = 0; i < children.Count(); i++)
            {
                string unit = children.ElementAt(i).Key;
                if (unit != "Version")
                {
                    unitEntries.Add(new UnitEntry(unit));
                }
            }
        }

        public void writeTooltips()
        {
            List<string> langFiles = getAddonLangPaths();

            for (int l = 0; l < langFiles.Count(); l++)
            {
                string file = langFiles.ElementAt(l);

                string thisLang = file.Substring(file.LastIndexOf('\\') + 1);
                string thisLangCopy = thisLang;
                thisLang = thisLang.Substring(thisLang.LastIndexOf('_') + 1);
                string outputPath = Path.Combine(GamePath, "resource", "tooltips_" + thisLang);

                alreadyHasKeys.Clear();

                KeyValue[] addon_lang = KVParser.KV1.ParseAll(File.ReadAllText(file));

                if (addon_lang.Count() > 0)
                {
                    IEnumerable<KeyValue> rootChildren = addon_lang[0].Children;

                    for (int k = 0; k < rootChildren.Count(); k++)
                    {
                        KeyValue child = rootChildren.ElementAt(k);
                        if (child.HasChildren)
                        {
                            IEnumerable<KeyValue> children2 = child.Children;
                            for (int j = 0; j < children2.Count(); j++)
                            {
                                KeyValue child2 = children2.ElementAt(j);
                                alreadyHasKeys.Add(child2.Key.ToLower());
                            }
                        }
                    }
                }

				StringBuilder content = new StringBuilder();

                // WriteAllText will clear the contents of this file first
                string header =
                    "// **********************************************************************************************************************\n" +
                    "// This file contains generated tooltips created from the files in the scripts/npc directory of this mod.\n" +
                    "// It does not contain tooltips already defined in " + thisLangCopy +
                    ", nor modifiers with the property \"IsHidden\" \"1\".\n" +
                    "// **********************************************************************************************************************\n";
				content.Append(header);


                string head1 = "\n\t\t// ******************** HEROES ********************\n";
				content.Append(head1);
                for (int i = 0; i < heroesEntries.Count(); i++)
                {
                    HeroEntry hero = heroesEntries.ElementAt(i);
                    if (!alreadyHasKeys.Contains(hero.Name.Key.ToLower()))
                    {
						content.Append(hero.ToString());
                    }
                }

                string head2 = "\n\t\t// ******************** UNITS ********************\n";
				content.Append(head2);
                for (int i = 0; i < unitEntries.Count(); i++)
                {
                    UnitEntry unit = unitEntries.ElementAt(i);
                    if (!alreadyHasKeys.Contains(unit.Name.Key.ToLower()))
                    {
						content.Append(unit.ToString());
                    }
                }

                string head3 = "\n\t\t// ******************** ABILITY MODIFIERS ********************\n";
				content.Append(head3);
                for (int i = 0; i < modifierAbilityKeys.Count(); i++)
                {
                    ModifierEntry mod = new ModifierEntry(modifierAbilityKeys.ElementAt(i));
                    if (!alreadyHasKeys.Contains(mod.Name.Key.ToLower()))
                    {
						content.Append(mod + "\n");
                    }
                }

                string head6 = "\n\t\t// ******************** ITEM MODIFIERS ********************\n";
				content.Append(head6);
                for (int i = 0; i < modifierItemKeys.Count(); i++)
                {
                    //ModifierEntry mod = modifierItemEntries.ElementAt(i);
                    ModifierEntry mod = new ModifierEntry(modifierItemKeys.ElementAt(i));
                    if (!alreadyHasKeys.Contains(mod.Name.Key.ToLower()))
                    {
						content.Append(mod + "\n");
                    }
                }

                string head4 = "\n\t\t// ******************** ABILITIES ********************\n";
				content.Append(head4);
                for (int i = 0; i < abilityEntries.Count(); i++)
                {
                    AbilityEntry abil = abilityEntries.ElementAt(i);
                    if (!alreadyHasKeys.Contains(abil.Name.Key.ToLower()))
                    {
						content.Append(abil + "\n");
                    }
                    else
                    {
                        // the addon_language already has this ability. but let's check
                        // if there are any new AbilitySpecials.
						bool missingAbilSpecials = false;
                        foreach (Pair p in abil.AbilitySpecials)
                        {
                            if (!alreadyHasKeys.Contains(p.Key.ToLower()))
                            {
                                // the addon_language doesn't contain this abil special.
								content.Append(p.ToString());
								missingAbilSpecials = true;
                            }
                        }
						if (missingAbilSpecials) {
							content.Append("\n");	
						}
                    }
                }

                string head5 = "\n\t\t// ******************** ITEMS ********************\n";
				content.Append(head5);
                for (int i = 0; i < itemEntries.Count(); i++)
                {
                    AbilityEntry item = itemEntries.ElementAt(i);
                    if (!alreadyHasKeys.Contains(item.Name.Key.ToLower()))
                    {
						content.Append(item + "\n");
                    }
                    else
                    {
                        // the addon_language already has this ability. but let's check
                        // if there are any new AbilitySpecials.
						bool missingAbilSpecials = false;
                        foreach (Pair p in item.AbilitySpecials)
                        {
                            if (!alreadyHasKeys.Contains(p.Key.ToLower()))
                            {
                                // the addon_language doesn't contain this abil special.
								content.Append(p.ToString());
								missingAbilSpecials = true;
                            }
                        }
						if (missingAbilSpecials) {
							content.Append("\n");
						}
                    }
                }

                // open the tooltips.txt in a text editor
				File.WriteAllText(outputPath, content.ToString(), Encoding.Unicode);
                Process.Start(outputPath);
            }
        }

        public List<string> getAddonLangPaths()
        {
            string[] resourceFiles = Directory.GetFiles(Path.Combine(GamePath, "resource"));
            List<string> langFiles = new List<string>();

            // only take the addon_language files
            for (int i = 0; i < resourceFiles.Count(); i++)
            {
				string resourceFile = resourceFiles[i];
				if (resourceFile.Contains("addon_") && resourceFile.EndsWith(".txt") && !resourceFile.EndsWith("utf8.txt"))
                {
                    langFiles.Add(resourceFile);
                }
            }
            return langFiles;
        }

		public KeyValue KVData { get; set; }

		public bool create_note0_lore { get; set; }
	}
}