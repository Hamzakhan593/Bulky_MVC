$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    $('#tblData').DataTable({
        ajax: {
            url: '/admin/product/getall',
            dataSrc: 'data' // This matches your { data: objProductList } structure
        },
        columns: [
            { data: 'title', width: '15%' },
            { data: 'author', width: '15%' },
            { data: 'isbn', width: '15%' },
            { data: 'listPrice', width: '10%' },
            { data: 'price', width: '10%' },
            { data: 'price50', width: '10%' },
            { data: 'price100', width: '10%' },
            {
                data: 'category.categoryName',
                width: '15%',
                defaultContent: 'N/A' // Shows when category is null
            },
            {
                data: 'productId',
                render: function (data) {
                    return `
                        <div class="btn-group">
                            <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a href="/admin/product/delete?id=${data}" class="btn btn-danger mx-2">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>`;
                },
                width: '15%'
            }
        ],
        initComplete: function () {
            console.log('DataTable initialized'); // Debugging
        },
        error: function (xhr, error, thrown) {
            console.error('Error loading data:', error, thrown);
        }
    });
}