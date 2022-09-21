## Transform samples 

These samples show how Power Fx rules can do transforms.


### Transform a Record into a table row.
This transform an arbitrary json object into a flat record . 

Run: `TransformFx.exe Record.json RecordTransform.fx.yaml`

**Input Record:**
```
{
    "Person": {
        "First": "John",
        "Last": "Smith"
    },
    "Score": 200,
    "DistanceMiles" :  57
}
```

**Power Fx:**
```
FirstName: =Person.First
LastName: =Person.Last
Score: = Score
DistanceKilo : =DistanceMiles / 1.609344
```


**Output result:**
```
FirstName: John
LastName: Smith
Score: 200
DistanceKilo : 35.41815795752803
```


### Transform a table with calculated columns
This creates calculated columns in a table. 

Run: `TransformFx.exe Table.json TableTransform.fx.yaml`

**Input table**
```
Name,Score,Score2
Bob,100,150
Fred,200,220
John,300,220
```

**Power Fx:**
```
Diff: = Score2 - Score
Name2: = Name & Diff
Total: = PreviousRecord.Total + Score
RowDiff: = Score - PreviousRecord.Score
```

**Output Table** 
```
Name,Score,Score2,Diff,Name2,Total,RowDiff
Bob,100,150,50,Bob50,100,100
Fred,200,220,20,Fred20,300,100
John,300,220,-80,John-80,600,100
```

