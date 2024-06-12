<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<script>
    function storeSearchHistory() {
        var items = [];
        $('#searchhistory li').each(function() {
            items.push($(this).text());
        });
        localStorage.setItem('searchHistory', JSON.stringify(items));
    }

    // Call this function before navigating to the new page
    // Example: <a href="newpage" onclick="storeSearchHistory()">Navigate</a>
</script>

<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<script>
    $(document).ready(function() {
        var storedHistory = localStorage.getItem('searchHistory');
        if (storedHistory) {
            var items = JSON.parse(storedHistory);
            var $myHistory = $('#myhistory');
            items.forEach(function(item) {
                var $li = $('<li>').text(item);
                $myHistory.append($li);
            });
        }
    });
</script>
