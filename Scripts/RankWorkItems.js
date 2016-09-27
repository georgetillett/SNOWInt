var currentRow = null;
var rankTextboxRowIndex = 3;
var firstRowIndex = 1;

	function selectRow(row)
	{
		// Clear the SELECTED style on the previous Current row
		if (currentRow != null)
		{
			currentRow.className = "";
		}
		
		// Set the NEW current row
		currentRow = row;
		currentRow.className = "selectedRow";
		
		setAllRowStyles(row.parentNode.parentNode);
	}
	function moveUp()
	{
		if (currentRow == null) return false;
		
		// Get all ORDER values
		var orderValues = getAllValues(currentRow.parentNode.parentNode);
		
		var thisRow = currentRow;
		
		// Find the row just above this row
		var targetRow = null;
		var nodes = thisRow.parentNode.childNodes;
		for (i = firstRowIndex; i < nodes.length; i++) 
		{
			if (nodes[i] == thisRow )
			{
				break;
			}
			targetRow= nodes[i];
		}	
		if (targetRow == null)
		{
			return false;
		}
		
		// Change the Row style
		changeRowStyle(targetRow);
		
		// Move the row
        thisRow.parentNode.insertBefore (thisRow, targetRow);
		
		// Reset the "Order" values
		setAllValues(currentRow.parentNode.parentNode, orderValues);
		
		return false;
	}
	function moveDown()
	{
		if (currentRow == null) return false;
		
		// Get all ORDER values
		var orderValues = getAllValues(currentRow.parentNode.parentNode);
	
		var thisRow = currentRow;
	
		// Find the row just below this row
		var targetRow = null;
		var nodes = thisRow.parentNode.childNodes;
		for (i = firstRowIndex; i < nodes.length; i++) 
		{
			if (nodes[i] == thisRow )
			{
				// If this is not last row, then select the next row
				if (i != nodes.length)
				{
					targetRow= nodes[i+1];
				}
				break;
			}
		}	
		if (targetRow == null)
		{
			return false;
		}
		
		// Change the Row style
		changeRowStyle(targetRow);
		
		// Move the row
        thisRow.parentNode.insertBefore (targetRow, thisRow);
		
		// Reset the "Order" values
		setAllValues(currentRow.parentNode.parentNode, orderValues);
		
		return false;
	}
	function moveTop()
	{
		if (currentRow == null) return false;
		
		// Get all ORDER values
		var orderValues = getAllValues(currentRow.parentNode.parentNode);
		
		// Find the first row
		var thisRow = currentRow;
		var targetRow = thisRow.parentNode.childNodes[firstRowIndex];
		
		// Move the row
        thisRow.parentNode.insertBefore (thisRow, targetRow);
		
		// Change the Row style
		setAllRowStyles(targetRow.parentNode.parentNode);
		
		// Reset the "Order" values
		setAllValues(currentRow.parentNode.parentNode, orderValues);
		
		return false;
	}
	function moveBottom()
	{
		if (currentRow == null) return false;
		
		// Get all ORDER values
		var orderValues = getAllValues(currentRow.parentNode.parentNode);
		
		// Find the last row
		var thisRow = currentRow;
		var targetRow = thisRow.parentNode.childNodes[thisRow.parentNode.childNodes.length - 1];
		
		// Swap the rows
        thisRow.parentNode.insertBefore (thisRow, targetRow.nextSibling);
		
		// Change the Row style
		setAllRowStyles(targetRow.parentNode.parentNode);
		
		// Reset the "Order" values
		setAllValues(currentRow.parentNode.parentNode, orderValues);
		
		return false;
	}
	function getTextboxNode(row)
	{
		// Find the ONE node in the row that is a TEXTBOX
	    //	   This is the "@Order" value, which will be saved
	    var textbox = row.cells[rankTextboxRowIndex].getElementsByTagName("INPUT")[0];
	    return textbox;
	}
	function changeRowStyle(row)
	{
		if (row.className == "alternateRow")
		{
			row.className = "";
		}
		else
		{
			row.className = "alternateRow";
		}	
	}
	function setAllRowStyles(table)
	{
	    //var rows = table.childNodes[1].childNodes;
	    var rows = table.getElementsByTagName("TR");

		var currentStyle = "alternateRow";

		for(i=0; i<rows.length; i++) 
		{
			// Set the class on this row, but only if it is not "Selected"
			if (rows[i].className != "selectedRow" )
			{
				rows[i].className = currentStyle;
			}
			
			// Swap styles
			if (currentStyle == "alternateRow")
			{
				currentStyle = "";
			}
			else
			{
				currentStyle = "alternateRow";
			}	
		}	
	}
	function getAllValues(table)
	{
		var orderValues = [];
		//var rows = table.childNodes[1].childNodes;
		var rows = table.getElementsByTagName("TR");

		for (index = firstRowIndex; index < rows.length; index++) 
        {
		    var orderTextbox = getTextboxNode(rows[index]);
		    orderValues.push(orderTextbox.value);
		}
		return orderValues;
	}
	function setAllValues(table, orderValues)
	{
		//var rows = table.childNodes[1].childNodes;
	    var rows = table.getElementsByTagName("TR");
		
		// Ensure the same number of rows and orders
	    if (rows.length - firstRowIndex != orderValues.length)
		{
			alert("ERROR: The number of Rank values does not match the number of ROWS.");
			return;
		}

		// Reassign the values, top to bottom, starting with row 1
        for (index = 0; index < orderValues.length; index++) 
		{
		    var orderTextbox = getTextboxNode(rows[index + firstRowIndex]); // adjusting for the Header row
			orderTextbox.value = orderValues[index];
		}
	}
