<?php $this->load->view('common/header'); ?>

<div class="container-fluid">
    <h2>端末一覧</h2>
    <?php
    $attributes = array('class' => 'form-inline', 'role' => 'form', 'method' => 'get');
    echo form_open('box', $attributes);
    ?>
	    <?php echo validation_errors(); ?>
	    <div class="panel panel-info">
	        <div class="panel-heading">
	            <div class="panel-title">検索</div>
	        </div>
	        <div class="panel-body">
	            <table class="table table-bordered">
	                <tr>
	                    <th>設置都道府県</th>
	                    <td>
	                    	<select name="pref">
		                    <?php
		                    foreach ($prefs as $value) {
		                    	$selected = $value == $pref_selected ? " selected" : "";
		                    ?>
		                    	<option value="<?php echo $value; ?>"<?php echo $selected; ?>><?php echo $value; ?></option>
		                    <?php
		                    }
		                    ?>
	                    	</select>
	                    </td>
	                </tr>
	                <tr>
	                    <th>登録日</th>
	                    <td>
	                        <div class="input-group">
	                            <!--<span class="input-group-addon"><i class="glyphicon glyphicon-calendar"></i></span>-->
	                            <input id="datepicker1" class="input-sm" name="entry_date_over" type="text" placeholder="登録日（以上）" value="<?php echo $entry_date_over; ?>" readonly/>
	                        </div>
	                        <div class="input-group">
	                            &nbsp;～&nbsp;
	                            <!--<span class="input-group-addon"><i class="glyphicon glyphicon-calendar"></i></span>-->
	                            <input id="datepicker2" class="input-sm" name="entry_date_under" type="text" placeholder="登録日（以下）" value="<?php echo $entry_date_under; ?>" readonly/>
	                        </div>
	                    </td>
	                </tr>
	            </table>
	        </div>
	        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-info btn-lg"><i class="glyphicon glyphicon-search" onclick="search_user"></i>検索</button></div>
	    </div>
	</form>
	<table class="table table-hover">
	    <tr class="success">
	        <th>端末ID</th>
	        <th>登録日</th>
	        <th>経度</th>
	        <th>緯度</th>
	    </tr>
	    <?php if (!empty($list)) { ?>
	        <?php foreach ($list as $val) { ?>
	            <tr>
	                <td><?php echo $val->id ?></td>
	                <td><?php echo $val->entry_date ?></td>
	                <td><?php echo $val->x ?></td>
	                <td><?php echo $val->y ?></td>
	            </tr>
	            <?php
	        }
	    }
	    ?>
	</table>
</div>


<?php $this->load->view('common/footer'); ?>
