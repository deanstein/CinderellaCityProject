using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides a variety of utilities to read CSV files
/// and return formatted data from them
/// </summary>

public class ReadCSV
{
    // read a CSV file and return an organized list of data for the credits screen
    public static List<List<string>> GetCreditsListsFromCSV()
    {
        string csvPath = UIGlobals.relativeUIPath + "collaborators";

        TextAsset csvData = (TextAsset)Resources.Load(csvPath, typeof(TextAsset));

        string[] rows = csvData.text.Split(new char[] { '\n' });

        // the row of the CSV where the data actually starts
        int startingRow = 9;

        // create a new list of data that eliminates the empty cells
        List<string> newList = new List<string>();

        // the final list of lists
        List<List<string>> csvLists = new List<List<string>>();

        for (int i = startingRow; i < rows.Length; i++)
        {
            string[] rowData = rows[i].Split(new char[] { ',' });
            string rowDataFlattened = string.Join("", rowData);

            // add an empty list for this row
            List<string> rowDataList = new List<string>();

            if (rowDataFlattened.Length > 3)
            {
                csvLists.Add(rowDataList);
            }

            foreach (string cellData in rowData)
            {
                if (cellData.Length > 3)
                {
                    string newData = cellData.Replace("\"", "");
                    rowDataList.Add(newData);
                }
            }
        }

        return csvLists;
    }
}