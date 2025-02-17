﻿using ProcGenGame;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClusterTraitGenerationManager.STRINGS.UI.CGM.INDIVIDUALSETTINGS;
using static STRINGS.BUILDINGS.PREFABS;

namespace ClusterTraitGenerationManager
{
    internal class STRINGS
    {
        public class WORLD_TRAITS
        {
            public class CGM_RANDOMTRAIT
            {
                public static LocString NAME = (LocString)"Randomized Traits";
                public static LocString DESCRIPTION = (LocString)"Chooses between 1 and 3 Traits at random.\nMutually exclusive with other selectable Traits.";
            }
        }
        public class UI
        {
            public class GENERATIONWARNING
            {
                public static LocString WINDOWNAME = (LocString)"Potential Generation Errors detected!";
                public static LocString DESCRIPTION = (LocString)"You have selected more than 6 outer planets, which can lead to placement failures.\n Automatically adjust cluster size and placements?";
                public static LocString YES = (LocString)"Yes";
                public static LocString NOMANUAL = (LocString)"No, let me do it manually.";
            }

            public class PRESETWINDOWCLUSTERPRESETS
            {
                public class DELETEWINDOW
                {
                    public static LocString TITLE = "Delete {0}";
                    public static LocString DESC = "You are about to delete the preset \"{0}\".\nDo you want to continue?";
                    public static LocString YES = "Confirm Deletion";
                    public static LocString CANCEL = "Cancel";

                }

                public static LocString TITLE = "Cluster Presets";

                public class HORIZONTALLAYOUT
                {
                    public class OBJECTLIST
                    {
                        public class SCROLLAREA
                        {
                            public class CONTENT
                            {
                                public class NOPRESETSAVAILABLE
                                {
                                    public static LocString LABEL = "No presets available";
                                }
                                public class PRESETENTRYPREFAB
                                {
                                    public class ADDTHISTRAITBUTTON
                                    {
                                        public static LocString TEXT = "Load Preset";
                                        public static LocString TOOLTIP = "Load this preset to the preview";

                                    }

                                    public static LocString RENAMEPRESETTOOLTIP = "Rename Preset";
                                    public static LocString DELETEPRESETTOOLTIP = "Delete Preset";

                                }
                            }
                        }

                        internal class SEARCHBAR
                        {
                            public static LocString CLEARTOOLTIP = "Clear search bar";
                            public static LocString OPENFOLDERTOOLTIP = "Open the folder where the presets are stored.";
                            internal class INPUT
                            {
                                public class TEXTAREA
                                {
                                    public static LocString PLACEHOLDER = "Enter text to filter presets...";
                                    public static LocString TEXT = "";
                                }
                            }
                        }
                    }
                    public class ITEMINFO
                    {
                        public class BUTTONS
                        {
                            public class CLOSEBUTTON
                            {
                                public static LocString TEXT = "Return";
                                public static LocString TOOLTIP = "Close this preset window";
                            }
                            public class GENERATEFROMCURRENT
                            {
                                public static LocString TEXT = "Generate Preset";
                                public static LocString TOOLTIP = "Save the currently loaded cluster configuration to a new preset.";
                            }
                            public class APPLYPRESETBUTTON
                            {
                                public static LocString TEXT = "Apply Preset";
                                public static LocString TOOLTIP = "Apply the preset thats currently displayed in the preview to the custom cluster.";
                            }
                        }
                    }
                }
            }


            public class CGM
            {
                public class INDIVIDUALSETTINGS
                {
                    public class STARMAPITEMENABLED
                    {
                        public static LocString LABEL = (LocString)"Generate Item:";
                        public static LocString TOOLTIP = (LocString)"Should this asteroid/POI be generated at all?";
                    }
                    public class AMOUNTSLIDER
                    {
                        public class DESCRIPTOR
                        {
                            public static LocString LABEL = (LocString)"Number to generate:";
                            public static LocString TOOLTIP = (LocString)"How many instances of these should be generated.\nValues that aren't full numbers represent a chance to generate for POIs.\n(f.e. 0.8 = 80% chance to generate this POI)";
                            public static LocString OUTPUT = (LocString)"REPLAC";
                            public class INPUT
                            {
                                public static LocString TEXT = (LocString)"";

                            }
                        }
                    }
                    public class MINDISTANCESLIDER
                    {
                        public class DESCRIPTOR
                        {
                            public static LocString LABEL = (LocString)"Minimum Distance:";
                            public static LocString TOOLTIP = (LocString)"The minimum distance this asteroid has to the center of the starmap.";
                            public class INPUT
                            {
                                public static LocString TEXT = (LocString)"";
                            }
                        }
                    }
                    public class MAXDISTANCESLIDER
                    {
                        public class DESCRIPTOR
                        {
                            public static LocString LABEL = (LocString)"Maximum Distance:";
                            public static LocString TOOLTIP = (LocString)"The maximum distance this asteroid has to the center of the starmap.";
                            public class INPUT
                            {
                                public static LocString TEXT = (LocString)"";

                            }
                        }
                    }
                    public class BUFFERSLIDER
                    {
                        public class DESCRIPTOR
                        {
                            public static LocString LABEL = (LocString)"Buffer Distance:";
                            public static LocString TOOLTIP = (LocString)"The minimum distance this asteroid has to other asteroids.";
                            public class INPUT
                            {
                                public static LocString TEXT = (LocString)"";

                            }
                        }
                    }

                    public class METEORSEASON
                    {
                        public static LocString LABEL = (LocString)"Meteors:";
                        public static LocString TOOLTIP = (LocString)"What kind of meteors should come down on this asteroid?";
                        public static LocString NOSEASONSSELECTED = "No season types selected";
                        public static LocString ADDNEWSEASON = (LocString)"Add additional meteor season";
                        public static LocString ADDNEWSEASONTOOLTIP = (LocString)"Add another type of meteor season type to this asteroid.\nWarning: Meteor season types are all active at the same time,\nleading to a high volume of meteors at the same time if multiple are added.\nNormal asteroids have usually one season type,";
                        public static LocString ACTIVESEASONSELECTORLABEL = (LocString)"Active Meteor Seasons:";
                        public static LocString ACTIVEMETEORSLABEL = (LocString)"Active Meteor Showers:";

                        public class SEASONSELECTOR
                        {
                            public static LocString TITEL = (LocString)"Available Meteor Season Types:";
                            public static LocString ADDSEASONTYPEBUTTONLABEL = (LocString)"Add season type";
                            public static LocString NOSEASONTYPESAVAILABLE = (LocString)"No more available Season Types";
                            public static LocString SEASONTYPETOOLTIP = (LocString)"This season type contains the following shower types:";
                            public static LocString SEASONTYPENOMETEORSTOOLTIP = (LocString)"This season type does not contain any meteor shower types.\nThis might change in the future if Klei decides to add some showers to this season type.\nFor this reason the season type is listed here.";
                        }

                        public static LocString SWITCHTOOTHERSEASONTOOLTIP = (LocString)"Swap this season to a different type";
                        public static LocString REMOVESEASONTOOLTIP = (LocString)"Remove this season type";

                        public static LocString NOMETEORSHOWERS = (LocString)"No Meteor Showers";
                        public static LocString SHOWERTOOLTIP = (LocString)"Meteors in this shower type:";
                    }

                    public class ASTEROIDSIZEINFO
                    {
                        public static LocString LABEL = (LocString)"Asteroid Size:";
                        public static LocString WIDTH = (LocString)"Width:";
                        public static LocString HEIGHT = (LocString)"Height:";
                        public static LocString TOOLTIP = (LocString)"The dimensions of this asteroid.";
                        public static LocString SIZEWARNING = (LocString)"Warning!\nThe planet size you have selected has {0}% more area than a normal vanilla size asteroid.\nThis might lead to a low game performance!";


                        public class SIZESELECTOR
                        {

                            public static LocString NEGSIZE0 = (LocString)"Tiny";
                            public static LocString NEGSIZE0TOOLTIP = (LocString)"The asteroid is at 25% of its usual size.";
                            public static LocString NEGSIZE1 = (LocString)"Smaller";
                            public static LocString NEGSIZE1TOOLTIP = (LocString)"The asteroid is at 40% of its usual size.";
                            public static LocString NEGSIZE2 = (LocString)"Small";
                            public static LocString NEGSIZE2TOOLTIP = (LocString)"The asteroid is at 55% of its usual size.";
                            public static LocString NEGSIZE3 = (LocString)"Slightly Smaller";
                            public static LocString NEGSIZE3TOOLTIP = (LocString)"The asteroid is at 75% of its usual size.";

                            public static LocString SIZE0 = (LocString)"Normal";
                            public static LocString SIZE0TOOLTIP = (LocString)"The asteroid is at its usual size.";
                            public static LocString SIZE1 = (LocString)"Slightly Larger";
                            public static LocString SIZE1TOOLTIP = (LocString)"The asteroid is 25% larger than normal.";
                            public static LocString SIZE2 = (LocString)"Large";
                            public static LocString SIZE2TOOLTIP = (LocString)"The asteroid is 50% larger than normal.";
                            public static LocString SIZE3 = (LocString)"Huge";
                            public static LocString SIZE3TOOLTIP = (LocString)"The asteroid has twice its usual size.";
                            public static LocString SIZE4 = (LocString)"Massive";
                            public static LocString SIZE4TOOLTIP = (LocString)"The asteroid has three times its usual size.";
                            public static LocString SIZE5 = (LocString)"Enormous";
                            public static LocString SIZE5TOOLTIP = (LocString)"The asteroid has four times its usual size.";
                        }
                        public class RATIOSELECTOR
                        {

                            public static LocString NORMAL = (LocString)"Normal Shape";
                            public static LocString NORMALTOOLTIP = (LocString)"The asteroid has its usual shape.";
                            public static LocString WIDE1 = (LocString)"Slightly Wider";
                            public static LocString WIDE1TOOLTIP = (LocString)"The asteroid is a bit wider than normal.";
                            public static LocString WIDE2 = (LocString)"Wider";
                            public static LocString WIDE2TOOLTIP = (LocString)"The asteroid is wider than normal.";
                            public static LocString WIDE3 = (LocString)"Much Wider";
                            public static LocString WIDE3TOOLTIP = (LocString)"The asteroid is a lot wider than normal.";

                            public static LocString HEIGHT1 = (LocString)"Slightly Taller";
                            public static LocString HEIGHT1TOOLTIP = (LocString)"The asteroid is a bit taller than normal.";

                            public static LocString HEIGHT2 = (LocString)"Taller";
                            public static LocString HEIGHT2TOOLTIP = (LocString)"The asteroid is taller than normal.";

                            public static LocString HEIGHT3 = (LocString)"Much Taller";
                            public static LocString HEIGHT3TOOLTIP = (LocString)"The asteroid is a lot taller than normal.";

                        }


                        public class INPUT
                        {
                            public static LocString TEXT = (LocString)"";

                        }
                    }
                    public class CLUSTERSIZE
                    {
                        public class DESCRIPTOR
                        {
                            public static LocString TOOLTIP = (LocString)"The radius of the starmap.";
                            public static LocString LABEL = (LocString)"Cluster Size:";
                            public class INPUT
                            {
                                public static LocString TEXT = (LocString)"";

                            }
                        }
                    }
                    public class ASTEROIDTRAITS
                    {
                        public static LocString LABEL = (LocString)"Asteroid Traits:";
                        public class LISTVIEW
                        {
                            public class NOTRAITSELECTEDINFO
                            {
                                public static LocString LABEL = (LocString)"No Traits";
                            }
                        }
                        public class ADDTRAITBUTTON
                        {
                            public static LocString TEXT = (LocString)"Add Trait";
                        }
                    }
                    public class BUTTONS
                    {
                        public class RESETCLUSTERBUTTON
                        {
                            public static LocString TEXT = (LocString)"Reset Cluster";
                            public static LocString TOOLTIP = (LocString)"Undo all changes you have made by reloading the cluster preset.";
                        }
                        public class RESETSELECTIONBUTTON
                        {
                            public static LocString TEXT = (LocString)"Reset Current Selection";
                            public static LocString TOOLTIP = (LocString)"Undo all changes you have made to the currently selected item.";
                        }
                        public class RETURNBUTTON
                        {
                            public static LocString TEXT = (LocString)"Return";
                            public static LocString TOOLTIP = (LocString)"Return to the previous screen.";
                        }
                        public class PRESETBUTTON
                        {
                            public static LocString TEXT = (LocString)"Cluster Presets";
                            public static LocString TOOLTIP = (LocString)"Create new or load your existing cluster presets";
                        }
                        public class SETTINGSBUTTON
                        {
                            public static LocString TEXT = global::STRINGS.UI.FRONTEND.COLONYDESTINATIONSCREEN.CUSTOMIZE;
                            public static LocString TOOLTIP = (LocString)"Open the game settings screen.";
                        }
                        public class GENERATECLUSTERBUTTON
                        {
                            public static LocString TEXT = (LocString)"Start modified Game";
                            public static LocString TOOLTIP = (LocString)"Start generating a modified Cluster based on selected parameters.\nModified Cluster Generation is only activated if this button here is used.";
                        }
                    }
                }

                public class TRAITPOPUP
                {
                    public static LocString TEXT = (LocString)"available Traits:";
                    public class SCROLLAREA
                    {
                        public class CONTENT
                        {
                            public class NOTRAITAVAILABLE
                            {
                                public static LocString LABEL = (LocString)"No Traits available";

                            }
                            public class LISTVIEWENTRYPREFAB
                            {
                                public static LocString LABEL = (LocString)"trait label";
                                public class ADDTHISTRAITBUTTON
                                {
                                    public static LocString TEXT = (LocString)"Add this trait";

                                }
                            }
                        }
                    }
                    public class CANCELBUTTON
                    {
                        public static LocString TEXT = (LocString)"Cancel";
                    }
                }
            }


            public class CGMBUTTON
            {
                public static LocString DESC = (LocString)"Start customizing the currently selected cluster.";
            }
            public class CUSTOMCLUSTERUI
            {
                public static LocString NAMECATEGORIES = (LocString)"Starmap Item Category";
                public static LocString NAMEITEMS = (LocString)"Starmap Items of this category";
                public static class CATEGORYENUM
                {
                    public static LocString START = (LocString)"Start Asteroid";
                    public static LocString WARP = (LocString)"Teleport Asteroid";
                    public static LocString OUTER = (LocString)"Outer Asteroids";
                    public static LocString POI = (LocString)"Points of Interest";
                }

            }

            public class SPACEDESTINATIONS
            {
                public static class CGM_RANDOM_STARTER
                {
                    public static LocString NAME = (LocString)"Random Start Asteroid";
                    public static LocString DESCRIPTION = (LocString)"The starting asteroid will be picked at random";
                }
                public static class CGM_RANDOM_WARP
                {
                    public static LocString NAME = (LocString)"Random Teleporter Asteroid";
                    public static LocString DESCRIPTION = (LocString)"The teleporter asteroid will be picked at random";
                }
                public static class CGM_RANDOM_OUTER
                {
                    public static LocString NAME = (LocString)"Random Outer Asteroid(s)";
                    public static LocString DESCRIPTION = (LocString)"Choose an amount of random outer asteroids.\n\nEach asteroid can only generate once";
                }
                public class CGM_RANDOM_POI
                {
                    public static LocString NAME = "Random POI";
                    public static LocString DESCRIPTION = "Choose an amount of POIs at random.\n\nDoes not roll unique POIs\n(Temporal Tear, Russel's Teapot)";
                }
            }
        }
        public class CLUSTER_NAMES
        {
            public class CGSM
            {
                public static LocString NAME = "CGM Cluster";
                public static LocString DESCRIPTION = "CGM Cluster";
            }
        }
        public class ERRORMESSAGES
        {
            public static LocString PLANETPLACEMENTERROR = "Starmap Generation Error!\n{0} could not be placed on the star map. Increase the maximum distance of of this asteroid to fix this issue.";
            public static LocString MISSINGWORLD = "Missing Worlds!\nThe preset cannot be loaded since its start world is missing.";
        }
    }
}

