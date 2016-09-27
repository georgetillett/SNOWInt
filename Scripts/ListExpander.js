	var ExpandChars = "[+]";
	var CollapseChars = "[-]";

	function expandCollapse(caller)
	{
	    // The "caller" should be a table cell (<td> element)
        // <table><tbody><tr><td>
		var table = caller.parentNode.parentNode.parentNode;
		
		var callerId = caller.parentNode.id;
		
		var newDisplayStyle = getNewDisplayStyle(caller.innerText);
		var newExpanderText = getNewExpanderText(caller.innerText);

		caller.innerText = newExpanderText;
		
		var allStoryRowsSame = true;
		
		for (var i = 0; i < table.rows.length; i++)
		{
			var row = table.rows[i];
			var rowId = row.id;

			if (isTaskRow(rowId))
			{
				if (isCallerChild(callerId, rowId))
				{
					row.style.display = newDisplayStyle;
				}
			}
			else
			{
				// If ANY Story row has a different Expander, then don't reset the Top Expander
				if (!isHeaderRow(rowId))
				{
					if (!hasSameExpanderText(row, newExpanderText))
					{
						allStoryRowsSame = false;
					}
				}
			}
		}

		// If all of the Story rows have the SAME expander, then set the Header row to the same one
		if (allStoryRowsSame)
		{
			var headerRow = document.getElementById("headerRow");
			headerRow.childNodes[0].innerText = newExpanderText;
		}

	}
	function hasSameExpanderText(row, expanderText)
	{
		var cellText = row.childNodes[0].innerText;
		if (cellText == expanderText || cellText == "")
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	function expandCollapseAll(caller)
	{
	    // The "caller" should be a table cell
        //  <table><tbody><tr><th>
	    var table = caller.parentNode.parentNode.parentNode;
				
		var newDisplayStyle = getNewDisplayStyle(caller.innerText);
		var newExpanderText = getNewExpanderText(caller.innerText);

		caller.innerText = newExpanderText ;
		
		for (var i = 0; i < table.rows.length; i++)
		{
			var row = table.rows[i];
			var rowId = row.id;

			if (isTaskRow(rowId))
			{
				row.style.display= newDisplayStyle;
			}
			else
			{
				if (row.childNodes[1].innerText != "")
				{
					row.childNodes[1].innerText = newExpanderText;
				}
			}
		}

	}
	
	function getNewDisplayStyle(expanderText)
	{
		if (expanderText.replace(' ','') == ExpandChars)
		{
			return "";
		}
		else
		{
			return "none";
		}
	}
	
	function getNewExpanderText(expanderText)
	{
		if (expanderText.replace(' ','') == ExpandChars)
		{
			return CollapseChars;
		}
		else
		{
			return ExpandChars;
		}
	}

	function isCallerChild(callerId, rowId)
	{
		if (rowId.lastIndexOf(callerId, 0) == 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	function isTaskRow(rowId)
	{
		if (rowId.indexOf("_T") > 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	function isHeaderRow(rowId)
	{
		if (rowId == "headerRow")
		{
			return true;
		}
		else
		{
			return false;
		}
	}
