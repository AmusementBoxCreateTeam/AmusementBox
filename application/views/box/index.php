<?php $this->load->view('common/header'); ?>

<div class="container-fluid">
    <h2>端末一覧</h2>
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
