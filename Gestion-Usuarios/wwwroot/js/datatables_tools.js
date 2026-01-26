/**
 * DataTableInitializer class for configuring and initializing DataTables with customizable options.
 */
class DataTableInitializer {
    /**
     * Constructor for DataTableInitializer.
     * @param {object} options - User-defined configuration options for DataTable.
     */
    constructor(options = {}) {
        // Default configuration options for DataTable
        const defaultOptions = {
            responsive: true,     // Enables responsive table layout
            paging: true,         // Enables pagination
            searching: true,      // Enables search/filter functionality
            info: true,           // Displays table information
            autoWidth: false,     // Disables automatic column width adjustment
            deferRender: true,    // Defers rendering for performance
            searchBuilder: false, // Disables searchBuilder by default
            colVis: false,        // Disables column visibility control by default
            colReorder: false,    // Disables column reordering by default
            columnsToHide: []     // Array of column indices to hide
        };

        // Merge default options with user-provided options
        Object.assign(this, defaultOptions, options);
    }

    /**
     * Returns the default configuration for DataTable.
     * @return {object} - Default configuration for DataTable.
     */
    get defaultConfig() {
        const buttons = [
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i>', // Export to Excel
                className: 'btn btn-success',
            },
            {
                extend: 'pdfHtml5',
                text: '<i class="fas fa-file-pdf"></i>', // Export to PDF
                className: 'btn btn-danger',
            }
        ];

        // Add SearchBuilder button with FontAwesome icon if enabled
        if (this.searchBuilder) {
            buttons.push({
                extend: 'searchBuilder',
            });
        }

        // Add column visibility control button if enabled
        if (this.colVis) {
            buttons.push('colvis');
        }

        return {
            responsive: this.responsive,    // Enables responsive table layout
            paging: this.paging,            // Enables pagination
            searching: this.searching,      // Enables search/filter functionality
            info: this.info,                // Displays table information
            autoWidth: this.autoWidth,      // Disables automatic column width adjustment
            deferRender: this.deferRender,  // Defers rendering for performance
            pageLength: 10,                 // Default number of rows per page
            /* dom: 'Bfrtip',    */              // Layout for buttons and table elements
            //dom: '<"row"<"col-md-6"f><"col-md-6"B>>rt<"row"<"col-md-6"i><"col-md-6"p>>',
            dom: '<"row"<"col-md-6 text-start"B><"col-md-6 text-end"f>>t<"row"<"col-md-6"i><"col-md-6"p>>',
            buttons: buttons,               // Button configurations
            colReorder: this.colReorder,    // Enables/disables column reordering
            columnDefs: this.columnsToHide.map(index => ({
                targets: index,
                visible: false               // Hides specified columns
            })),
            language: {
                searchBuilder: {
                    button: '<i class="fas fa-filter"></i>'
                }
            }
        };
    }

    /**
     * Initializes the DataTable with the specified selector and additional configuration.
     * @param {string} selector - Selector for the HTML table element to initialize.
     * @param {object} additionalConfig - Optional additional configuration.
     * @return {object} - Initialized DataTable instance.
     */
    initialize(selector, additionalConfig = {}) {
        // Combine the default configuration with any additional configuration provided
        const config = Object.assign({}, this.defaultConfig, additionalConfig);

        // Initialize the DataTable with the combined configuration
        return $(selector).DataTable(config);
    }

    /**
     * Rebuilds the searchBuilder, clears the DataTable, and redraws it.
     * @param {object} table - DataTable instance.
     */
    rebuildAndRedraw(table) {
        // Rebuild the searchBuilder if it exists
        if (table.searchBuilder) {
            table.searchBuilder.rebuild();
        }
        // Clear the table data and redraw it
        table.clear().draw();
    }
}

/**
 * Converts a JSON date string to a formatted date string.
 * @param {string} jsonDate - JSON date string in the format /Date(1705039200000)/.
 * @param {boolean} includeTime - If true, includes the time in the output.
 * @return {string} - Formatted date string in YYYY-MM-DD or YYYY-MM-DD HH:MM format.
 */
function convertDate(jsonDate, includeTime = false) {
    if (!jsonDate) return '';

    // Extract the timestamp from the JSON date string using a regular expression
    const timestamp = parseInt(jsonDate.replace(/\/Date\((\d+)\)\//, '$1'), 10);
    const date = new Date(timestamp);
    const [year, month, day] = [
        date.getFullYear(),
        String(date.getMonth() + 1).padStart(2, '0'), // Months are zero-based
        String(date.getDate()).padStart(2, '0')
    ];

    // If includeTime is true, format the date-time string
    if (includeTime) {
        const [hours, minutes] = [
            String(date.getHours()).padStart(2, '0'),
            String(date.getMinutes()).padStart(2, '0')
        ];
        return `${year}-${month}-${day} ${hours}:${minutes}`;
    }

    // If includeTime is false, return only the date part
    return `${year}-${month}-${day}`;
}