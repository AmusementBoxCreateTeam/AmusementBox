<?php $this->load->view('common/header'); ?>

<div class="container-fluid">
    <h2>端末登録</h2>
    <?php
    $attributes = array('class' => 'form-inline', 'role' => 'form', 'method' => 'get');
    echo form_open('box', $attributes);
    ?>
	    <?php echo validation_errors(); ?>
	    <div class="panel panel-info">
	        <div class="panel-heading">
	            <div class="panel-title">端末登録</div>
	        </div>
	        <div class="panel-body">
	            <table class="table table-bordered">
	                <tr>
	                    <th>経度</th>
	                    <td>
	                    	<input type="text" name="x" />
	                    </td>
	                </tr>
	                <tr>
	                    <th>緯度</th>
	                    <td>
	                    	<input type="text" name="y" />
	                    </td>
	                </tr>
	            </table>
	        </div>
	        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-info btn-lg"><i class="glyphicon glyphicon-search" onclick="search_user"></i>検索</button></div>
	    </div>
	</form>
</div>

<?php $this->load->view('common/footer'); ?>
