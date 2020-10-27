# UNSABLE TEST BRANCH of Nandonalts-Camping-Stuff
Nandonalt's Camping Stuff updated to v1.0
Sourced from steam with the following changes:

## Installation Instructions

### To install a non-workshop RimWorld mod from zip download:
Not intended for installation

### TODO
* Change bag properties such as weight and value based on the items packed into bag
* Write/ migrate code to deploy, undeploy, and repair bags

### Changes
v1.0
* Split tents into bag, cover, pole, and floor parts
* Parts can be packed into and removed from bags

v0.6.0
* v1.0 support

v0.5.2
* Fixed bills not working on portable stoves

v0.5.1
* Fixed portable stoves not being portable

v0.5.0
* b19 support
* Major XML rework to remove duplicated and unnecessary definitions
* Portable stoves now inherit from campfires- this means that portable stoves should get any recipe that campfires get
* Major rework to the tent generation code, it is now capable of generating tents of arbitrary rectangular (M x N) size and only requires definition of the South facing orientation in XML
* Added a new 7x9 Long tent- for all your mobile medical, prison, and housing needs
* Removed butchering spot as it is now a core feature

v0.4.0
* b18 support
* Exact changes can be seen on GitHub, it mostly involved reworking the job coding
* Converted xml defs to lower case
* Removed sleeping bag as it is now a core feature

v0.3.0
* Retroactively changed version number to comply with Semantic Versioning standards

Initial changes (a17) From Nandonalt's last version
* Removed the references to Microsoft.CSharp and System.Net.Http and setting .net target to 3.5 (these may just be my visual studio adding stuff on me)
* Changed 'stringToLines' to 'StringToLines' on CompTargetable_Tent.cs lines 53 & 195
* Changed 'getPlacements' to 'GetPlacements' on CompTargetable_Tent.cs lines 191, 306, & 530 and CompPackTents.cs line 93
* Added a '-1,' to CompTargetable_Tent.cs line 446 so it reads: return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(this.parent.Stuff), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 9999f, validator, null, -1, -1, false);
* Changed 'Scribe_Values.LookValue' to 'Scribe_Values.Look' on CompPackTent.cs lines 64 & 65
* Deleted CompPackTent.cs lines 179-204
* Changed CompPackTent.cs lines 153-164 to:
  nodoor = false;
  thingList2[i].Destroy(DestroyMode.Vanish);

### Licensing
Unsure of licensing info, as Nandonalt didn't originally publish anything in that respect.

### Thanks
* to Nandonalt for making the mod
* to jfredric for fixing my awful readme
* to all the wonderful people on the Rimworld Discord
