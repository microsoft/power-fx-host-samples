Add all AX modules under the Metadata folder.

Example tree structure:
$/Project/Trunk/Main/Metadata
+---MyMainModule
|   +---Descriptor
|   |       MyFirstModel.xml
|   |       MyOtherModel.xml
|   |
|   +---MyFirstModel
|   |   +---AxClass
|   |   |       MyClass.xml
|   |   |
|   |   \---AxTable
|   |           MyMainTable.xml
|   |
|   \---MyOtherModel
|       \---AxForm
|               MyOtherForm.xml
|
\---MyOtherModule
    +---Descriptor
    |       MyTestModel.xml
    |
    \---MyTestModel
        \---AxClass
                MyTestClass.xml

Please see https://ax.help.dynamics.com for more details.