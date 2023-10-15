# Logix Upload/Download Tool

This is a utility for uploading/downloading Rockwell Automation Logix processors. This tool
is not supported, endorsed, or affiliated in any way with Rockwell Automation or Allen-Bradley.

No guarentees are made by this tool or its authors. The primary reason this tool was developed
was to be able to use this in a testing environment for DevOps reasons and is not tested
for production environments. **ANY USE IS AT YOUR OWN RISK**.

What can this tool do?

- Upload a controller to an ACD file, and optionally include tag values.
- Download an ACD file to a controller.
- Print out some information about the ACD file

**This tool does not have a GUI!** It is command-line only.

Requirements:

In order to use this tool, you must have:

- Studio 5000 in the version of the controller you are working with. This must be installed
 on the machine this tool runs on. 
- .NET 6
- Microsoft Windows 10 or above

Because Studio 5000 is a Windows-Only program, this tool is also Windows-Only. This tool is
also tied to the x86 (32-bit) processor architecture. 

## Downloading an ACD to a Controller

To download to the controller, you would supply the tool:

`ludtool.exe download -f <ACDFile.acd>`

Optionally, you can use the following arguments:

- `-p` Supplies an explicit path, i.e. `AB_ETHIP-1\192.168.1.1\Backplane\1`
- `--prog-mode` Leaves the PLC in program mode after downloading
- `--forces-on` Enables forces
- `-v` Verbose output

The tool will instantiate the Logix version required and download to the controller. If
an optional path is supplied, it will download using that path. The controller slot must
match the slot that is configured in the project.

## Uploading from a Controller

To upload from a controller, the following arguments can be used:

`ludtool.exe upload -f <ACDFile.acd> -p "path_to_controller"`

If the file exists, it will be merged, otherwise it will be created. The tool uses the
latest version of Studio 5000 found on the computer to upload. Optionally these arguments
can be supplied:

- `-t` Specifies to also upload tag values

## Getting Project Info

The tool can print out information about the project, such as the name of the controller,
the description, memory used/free, project path, and other data. To get the information
in a project, the following command is used:

`ludtool.exe info -f <ACDFile.acd>`

## Getting Help

The tool can print detailed help information with the following commands:

`ludtool.exe --help`\
`ludtool.exe download --help`\
`ludtool.exe upload --help`\
`ludtool.exe info --help`

## Technial Details

This tool was not created through decompilation or reverse engineering. This was mostly
created through "noodling" with the methods with trial-and-error. Therefore, there are
is the possibility that the system is being used improperly and undefined results may
happen. It is your responsibility to ensure that the tool behaves correctly for your
use case before using in an industrial environment. 

The developers, contributors, and maintainers of this tool make no warranty, express
or implied as to the operation of this tool. Use at your own risk.

## Contributing

Want to contribute? The easiest way is to fork this repo and create a pull request!
Please feel free to open issues for any problems encountered.


