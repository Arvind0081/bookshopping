var dataTable;

$(document).ready(function(){
    loadDataTable();
})

function loadDataTable() {

    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url": "/Admin/Category/GetAll"
        },

        "columns": [
            { "data": "name", "width": "80%", "className": "dt-center" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                      <a href="/Admin/Category/Upsert/${data}" class="btn btn-info"><i class="far fa-edit"></i></a>
                      <a onclick=Delete("/Admin/Category/Delete/${data}") class="btn btn-danger"><i class="fas fa-trash-alt"></i></a>
               
                         </div> `;
                }

            }
        ]
    })
}

function Delete(url) {
    
    swal({
        title: "Want to delete record?",
        text: "Information Deleted Can not be recoverd",
        icon: "warning",
        buttons: true,
        dangerModel:true
    }).then((willdelete) => {
        if (willdelete) {

            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }

                }
            })
           
        }
    })
}