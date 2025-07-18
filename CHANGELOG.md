# v1.2.2
- Bugfix: tents fail to install and spam error log (due to null pointer in PoleFactors)

# v1.2.1
- Bugfix: tent bags not craft-able due to graphic

# v1.2.0
- RimWorld 1.6 support
- Added support for Royalty DLC
- New terrain textures for tent floors (commissioned from Orange Peel Assassin)
- Code cleanup & optimizations

# V1.1.0
- RimWorld 1.5 support
- Moved wall, door, and floor definitions to tent XMl specification
- Tent mats now spawn appropriately colored floor terrain
- Added tent mats to scenario editor
- Fixed tent floors not tracking damage
- Removed installed tent item from debug spawn menu (this prevents error state tents from being accidently spawned)

# v1.0.4
- RimWorld 1.4 support
- Added tag to hopefully prevented deployed tents from being hauled in rare cases
- Enforced load before cucumpear's embrasures


# v1.0.3
- Fixed bug with tent roof not spawning

# v1.0.2
- Fixed portable stove- re-added workgivers since they got lost with the v1 update and removed the inherited flame meditation type
- Added manifest for compatibility with Fluffy's [Mod Manager](https://github.com/fluffy-mods/ModManager)

# v1.0.1
- Temporary incompatibility with Combat Extended (resolved now)
- Removed option to build doors from tent walls

# v1.0.0
- RimWorld 1.1 - 1.3 support
- Total rewrite
	- Tents are now a bag item that must be packed with a cover and a number of tent poles (and optionally a tent mat).
		- Each cover decides the shape of the spawned tent
		- The number of tent poles is based on the tent cover (smaller covers require fewer poles to support them)
		- Tent poles can be created from both woody and metallic materials (or just plain wood logs in a pinch) and influence properties of the deployed tent walls (such as beauty and HP)
		- Packing a tent mat causes the tent to be installed with a +1 beauty floor (assuming that the install location doesn't already have an existing floor)
		- Items packed in a bag can be removed/ replaced/ upgraded at any time and will be returned to the ground if the bag is destroyed
	- Damaged walls/ doors are now remembered and travel with the tent cover
- New mod image (courtesy of Orange Peel Assassin)
  
# v0.6.0
- RimWorld 1.0 support

# v0.5.2
- Fixed bills not working on portable stoves

# v0.5.1
- Fixed portable stoves not being portable

# v0.5.0
- RimWorld b19 support
- Major XML rework to remove duplicated and unnecessary definitions
- Portable stoves now inherit from campfires- this means that portable stoves should get any recipe that campfires get
- Major rework to the tent generation code, it is now capable of generating tents of arbitrary rectangular (M x N) size and only requires definition of the South facing orientation in XML
- Added a new 7x9 Long tent- for all your mobile medical, prison, and housing needs
- Removed butchering spot as it is now a core feature

# v0.4.0
- RimWorld b18 support
- Exact changes can be seen on GitHub, it mostly involved reworking the job coding
- Converted xml defs to lower case
- Removed sleeping bag as it is now a core feature

# v0.3.0
- Retroactively changed version number to comply with Semantic Versioning standards

# Initial changes (a17) From Nandonalt's last version
- RimWorld a17 support