<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <h2>ユーザ一覧</h2>
    <?php
    $attributes = array('class' => 'form-inline', 'role' => 'form', 'method' => 'get');
    echo form_open('user', $attributes);
    ?>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">検索</div>
        </div>
        <div class="panel-body">
            <table class="table table-bordered">
                <tr>
                    <th>性別</th><td><label class="checkbox-inline"><input type="checkbox" name="gender[]" value="1" /> 男</label><label class="checkbox-inline"><input type="checkbox" name="gender[]" value="0" /> 女</label></td>
                </tr>
                <tr>
                    <th>年齢</th>
                    <td>
                        <input class="input-sm" name="age_over" type="text" placeholder="年齢（以上）"  value="<?php echo !empty($search['age_over']) ? $search['age_over'] : ''; ?>"/>
                        &nbsp;～&nbsp;
                        <input class="input-sm" name="age_under" type="text" placeholder="年齢（以下）" value="<?php echo !empty($search['age_under']) ? $search['age_under'] : ''; ?>"/>
                    </td>
                </tr>
                <tr>
                    <th>登録日</th>
                    <td>
                        <div class="input-group">
                            <!--<span class="input-group-addon"><i class="glyphicon glyphicon-calendar"></i></span>-->
                            <input id="datepicker1" class="input-sm"  name="entry_date_over" type="text" placeholder="登録日（以上）" readonly value="<?php echo !empty($search['entry_date_over']) ? $search['entry_date_over'] : ''; ?>"/>
                        </div>
                        <div class="input-group">
                            &nbsp;～&nbsp;
                            <!--<span class="input-group-addon"><i class="glyphicon glyphicon-calendar"></i></span>-->
                            <input id="datepicker2" class="input-sm" name="entry_date_under" type="text" placeholder="登録日（以下）" readonly value="<?php echo !empty($search['entry_date_under']) ? $search['entry_date_under'] : ''; ?>"/>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-info btn-lg"><i class="glyphicon glyphicon-search"></i>検索</button></div>
    </div>
    <input name="per_page" type="hidden" valeu="<?php echo !empty($per_page) ? $per_page : ''; ?>" />
</form>
<h4><?php echo $count_row.'件中&nbsp;'.$start_row.'～'.$end_row.'表示' ?></h4>
<table class="table table-hover">
    <tr class="success">
        <th>ニックネーム</th>
        <th>年齢</th>
        <th>性別</th>
        <th>登録日</th>
    </tr>
    <?php if (!empty($list)) { ?>
        <?php foreach ($list as $val) { ?>
            <tr>
                <td><a href="#"><?php echo $val->nickname ?></a></td>
                <td><?php echo $val->age ?></td>
                <td><?php echo $val->gender ?></td>
                <td><?php echo $val->entry_date ?></td>
            </tr>
            <?php
        }
    }
    ?>
</table>
<h4><?php echo $count_row.'件中&nbsp;'.$start_row.'～'.$end_row.'表示' ?></h4>
<?php echo !empty($paging)?$paging:'' ?>
</div>
<?php $this->load->view('common/footer'); ?>
